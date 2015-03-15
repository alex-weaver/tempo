using Tempo.SharedMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo.Scheduling
{
    /// <summary>
    /// A queue of work to be performed. TaskQueue uses an IScheduler to sequentially schedule work.
    /// </summary>
    public class TaskQueue
    {
        private class PendingTransaction
        {
            public readonly Transaction transaction;
            public readonly Action method;

            public PendingTransaction(Transaction transaction, Action method)
            {
                this.transaction = transaction;
                this.method = method;
            }
        }

        private class Cancellable<T>
        {
            public T value;
            public bool cancelled = false;

            public Cancellable(T value)
            {
                this.value = value;
            }
        }



        private readonly HashSet<Updateable> invalidUpdateables = new HashSet<Updateable>();
        private readonly Queue<Cancellable<Updateable>> invalidUpdateableQueue = new Queue<Cancellable<Updateable>>();



        private readonly Queue<Action> pendingActions = new Queue<Action>();

        private readonly HashSet<PendingTransaction> pendingTransactions = new HashSet<PendingTransaction>();

        private readonly IScheduler scheduler;
        private readonly UpdateScheduler updater;

        private readonly int ConstructionThreadId;

        /// <summary>
        /// Constructs a new TaskQueue with a given IScheduler.
        /// </summary>
        /// <param name="scheduler">The IScheduler to sequentially schedule work.</param>
        public TaskQueue(IScheduler scheduler)
        {
            this.ConstructionThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

            this.scheduler = scheduler;
            this.updater = new UpdateScheduler(scheduler, this.Update);
        }

        
        /// <summary>
        /// Constructs a new updateable with the given handler. Calling NeedsUpdate() on the return value will
        /// schedule the handler to be run. Until the handler is actually executed, any further calls to NeedsUpdate
        /// will have no effect.
        /// </summary>
        /// <param name="lifetime">The lifetime of the updateable. When the lifetime has ended, the handler will not be scheduled and
        /// further calls to NeedsUpdate will have no effect.</param>
        /// <param name="handler">The action to perform on each update.</param>
        /// <returns>A handle to allow the caller to schedule updates.</returns>
        public IUpdateableHandle CreateUpdateable(Lifetime lifetime, Action handler)
        {
            var updateable = new Updateable(handler);

            var handle = new AnonymousUpdateableHandle(() =>
            {
                if (lifetime.IsDead)
                    return;

                if(invalidUpdateables.Add(updateable))
                {
                    var container = new Cancellable<Updateable>(updateable);
                    lifetime.WhenDead(() => container.cancelled = true);
                    invalidUpdateableQueue.Enqueue(container);

                    updater.NeedsUpdate();
                }
            });

            handle.NeedsUpdate();

            return handle;
        }

        


        /// <summary>
        /// Schedule an action for immediate execution.
        /// </summary>
        /// <param name="method">The action to schedule.</param>
        public void Schedule(Action method)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != ConstructionThreadId)
            {
                scheduler.Run(() =>
                    {
                        Schedule(method);
                    });
            }
            else
            {
                pendingActions.Enqueue(method);
                updater.NeedsUpdate();
            }
        }

        /// <summary>
        /// Schedule an action for some point in the future, with a lifetime to allow cancellation.
        /// </summary>
        /// <param name="lifetime">The lifetime of the request.</param>
        /// <param name="targetTimeUtc">The time when the action will be scheduled.</param>
        /// <param name="method">The action to schedule.</param>
        public void ScheduleAt(Lifetime lifetime, DateTime targetTimeUtc, Action method)
        {
            Action handler = null;
            handler = new Action(() =>
            {
                if(DateTime.UtcNow >= targetTimeUtc)
                {
                    Schedule(method);
                }
                else
                {
                    scheduler.Run(handler);
                }
            });

            scheduler.Run(handler);
        }


        /// <summary>
        /// Schedule a transactional action. This method will attempt to lock all of the participants.
        /// If they can be locked, the action will be scheduled immediately. Otherwise, it will be put in
        /// a queue until all participants are available.
        /// </summary>
        /// <param name="method">The action to invoke when all participants have been locked.</param>
        /// <param name="participants">The participants to lock.</param>
        public void Transaction(Action method, params IHasParticipant[] participants)
        {
            var transaction = new Transaction(participants.Select(x => x.TransactionParticipant).ToArray());
            if(transaction.TryAcquireAll())
            {
                Schedule(() =>
                    {
                        method();
                        transaction.ReleaseAll();
                    });
            }
            else
            {
                pendingTransactions.Add(new PendingTransaction(transaction, method));
                updater.NeedsUpdate();
            }
        }

        private void Update()
        {
            while (pendingActions.Any())
            {
                var method = pendingActions.Dequeue();
                if (method != null)
                {
                    method();
                }
            }

            var toRemove = new List<PendingTransaction>();
            foreach(var transaction in pendingTransactions)
            {
                if(transaction.transaction.TryAcquireAll())
                {
                    Schedule(transaction.method);
                    transaction.transaction.ReleaseAll();
                    toRemove.Add(transaction);
                }
            }

            foreach(var transaction in toRemove)
            {
                pendingTransactions.Remove(transaction);
            }

            while (invalidUpdateableQueue.Any())
            {
                var container = invalidUpdateableQueue.Dequeue();
                var updateable = container.value;
                invalidUpdateables.Remove(updateable);

                if (!container.cancelled)
                {
                    updateable.Update();
                }
            }

            if (pendingTransactions.Any())
            {
                updater.NeedsUpdate();
            }
        }
    }
}

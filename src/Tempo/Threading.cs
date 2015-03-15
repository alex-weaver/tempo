using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo.SharedMemory;
using Tempo.Util;

namespace Tempo
{
    public static class Threading
    {
        /// <summary>
        /// Construct a new scope which runs in a new thread. When this scope ends, the thread's scope is ended, and the thread is terminated.
        /// </summary>
        /// <param name="constructor">The constructor for the thread's top-level scope.</param>
        public static void RunThread(Action constructor)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            ThreadBuilder.Build(callingScope.lifetime, constructor);
        }

        /// <summary>
        /// A combination of the Whilst and Thread operations, provided as a single method for code clarity.
        /// This is equivalent to calling Thread() inside a Whilst() operation. Note that this is slightly different
        /// to reversing the order and calling Whilst() inside a Thread() operation. The former is used because it does not require
        /// any synchronization of the threadActivity argument, and minimizes resource usage when the threadActivity cell is false.
        /// </summary>
        /// <param name="threadActivity">The condition cell.</param>
        /// <param name="constructor">The constructor for the thread's top-level scope.</param>
        public static void RunThreadWhilst(ICellRead<bool> threadActivity, Action constructor)
        {
            threadActivity.WhilstTrue(() =>
            {
                RunThread(constructor);
            });
        }



        /// <summary>
        /// Lock a shared variable for exclusive access. If the variable is already locked, the handler will
        /// be put in a queue until a lock can be obtained. This should only be called from a sequential block.
        /// </summary>
        /// <typeparam name="T">The type of the shared variable.</typeparam>
        /// <param name="s1">The shared variable</param>
        /// <param name="handler">The handler to invoke after a lock is obtained.</param>
        public static void Lock<T>(IShared<T> s1, Action<T> handler)
        {
            var taskQueue = CurrentThread.CurrentSequentialScope().taskQueue;
            taskQueue.Transaction(() => CurrentThread.RunSequentialBlock(() => handler(s1.Get())), s1);
        }

        /// <summary>
        /// Lock shared variables for exclusive access. If any of the variables are already locked, the handler will
        /// be put in a queue until a lock can be obtained. This should only be called from a sequential block.
        /// </summary>
        /// <typeparam name="T1">The type of the first shared variable.</typeparam>
        /// <typeparam name="T2">The type of the second shared variable.</typeparam>
        /// <param name="s1">The first shared variable</param>
        /// <param name="s2">The second shared variable</param>
        /// <param name="handler">The handler to invoke after a lock is obtained.</param>
        public static void Lock<T1, T2>(IShared<T1> s1, IShared<T2> s2, Action<T1, T2> handler)
        {
            var taskQueue = CurrentThread.CurrentSequentialScope().taskQueue;
            taskQueue.Transaction(() => CurrentThread.RunSequentialBlock(() => handler(s1.Get(), s2.Get())), s1, s2);
        }

        /// <summary>
        /// Lock shared variables for exclusive access. If any of the variables are already locked, the handler will
        /// be put in a queue until a lock can be obtained. This should only be called from a sequential block.
        /// </summary>
        /// <typeparam name="T1">The type of the first shared variable.</typeparam>
        /// <typeparam name="T2">The type of the second shared variable.</typeparam>
        /// <typeparam name="T3">The type of the third shared variable.</typeparam>
        /// <param name="s1">The first shared variable</param>
        /// <param name="s2">The second shared variable</param>
        /// <param name="s3">The third shared variable</param>
        /// <param name="handler">The handler to invoke after a lock is obtained.</param>
        public static void Lock<T1, T2, T3>(IShared<T1> s1, IShared<T2> s2, IShared<T3> s3, Action<T1, T2, T3> handler)
        {
            var taskQueue = CurrentThread.CurrentSequentialScope().taskQueue;
            taskQueue.Transaction(() => CurrentThread.RunSequentialBlock(() => handler(s1.Get(), s2.Get(), s3.Get())), s1, s2, s3);
        }

        /// <summary>
        /// Lock shared variables for exclusive access. If any of the variables are already locked, the handler will
        /// be put in a queue until a lock can be obtained. This should only be called from a sequential block.
        /// </summary>
        /// <typeparam name="T1">The type of the first shared variable.</typeparam>
        /// <typeparam name="T2">The type of the second shared variable.</typeparam>
        /// <typeparam name="T3">The type of the third shared variable.</typeparam>
        /// <typeparam name="T4">The type of the fourth shared variable.</typeparam>
        /// <param name="s1">The first shared variable</param>
        /// <param name="s2">The second shared variable</param>
        /// <param name="s3">The third shared variable</param>
        /// <param name="s4">The fourth shared variable</param>
        /// <param name="handler">The handler to invoke after a lock is obtained.</param>
        public static void Lock<T1, T2, T3, T4>(IShared<T1> s1, IShared<T2> s2, IShared<T3> s3, IShared<T4> s4, Action<T1, T2, T3, T4> handler)
        {
            var taskQueue = CurrentThread.CurrentSequentialScope().taskQueue;
            taskQueue.Transaction(() => CurrentThread.RunSequentialBlock(() => handler(s1.Get(), s2.Get(), s3.Get(), s4.Get())), s1, s2, s3, s4);
        }



        /// <summary>
        /// Observe a shared value on the current thread. A copy of the current value in the cell is kept on the
        /// calling thread, so the current value can always be read without locking.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sharedCell"></param>
        /// <returns></returns>
        public static ICellRead<T> WatchOnCurrentThread<T>(IShared<MemoryCell<T>> sharedCell)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            T shadow = default(T);

            Events.Once(() =>
            {
                Lock(sharedCell, value => shadow = value.Cur);
            });

            var listeners = new HashSet<Action>();
            sharedCell.TransactionParticipant.WhenUnlocked(callingScope.lifetime, () =>
            {
                var valueCopy = sharedCell.Get().Cur;

                callingScope.taskQueue.Schedule(() =>
                {
                    shadow = valueCopy;
                    foreach (var listener in listeners)
                    {
                        listener();
                    }
                });
            });

            return new AnonymousCellRead<T>(
                () => shadow,
                (lifetime, handler) =>
                {
                    listeners.Add(handler);
                    lifetime.WhenDead(() => listeners.Remove(handler));
                });
        }



        /// <summary>
        /// Observe a shared value on the current thread. A copy of the current value in the cell is kept on the
        /// calling thread, so the current value can always be read without locking.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sharedCell"></param>
        /// <returns></returns>
        public static IListCellRead<T> WatchOnCurrentThread<T>(IShared<IListCellRead<T>> sharedCell)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var shadow = new ListCell<T>();


            var changesQueue = new ConcurrentQueue<ListCellChanged<T>>();

            // ListCell<T>.WatchChanges must be called on the owning thread - it is not thread safe
            sharedCell.OwningScope.Schedule(() =>
            {
                var initialChange = new ListCellChanged<T>(ListChangeAction.Add, 0, sharedCell.Get().Cur.ToList(), -1, 0);
                callingScope.taskQueue.Schedule(() =>
                {
                    shadow.InsertRange(initialChange.NewStartingIndex, initialChange.NewItems);
                    initialChange.Release();
                });

                sharedCell.Get().ListenChanges(callingScope.lifetime, change =>
                {
                    if (!callingScope.lifetime.IsDead)
                    {
                        change.AddRef();
                        changesQueue.Enqueue(change);
                    }
                });
            });

            sharedCell.TransactionParticipant.WhenUnlocked(callingScope.lifetime, () =>
            {
                callingScope.taskQueue.Schedule(() =>
                {
                    ListCellChanged<T> change;
                    while (changesQueue.TryDequeue(out change))
                    {
                        switch (change.Action)
                        {
                            case ListChangeAction.Add:
                                shadow.InsertRange(change.NewStartingIndex, change.NewItems);
                                break;
                            case ListChangeAction.Remove:
                                shadow.RemoveRange(change.OldStartingIndex, change.OldItemCount);
                                break;
                            case ListChangeAction.Replace:
                                shadow.ReplaceRange(change.NewStartingIndex, change.NewItems);
                                break;
                        }

                        change.Release();
                    }
                });
            });

            callingScope.lifetime.WhenDead(() =>
            {
                ListCellChanged<T> change;
                while (changesQueue.TryDequeue(out change))
                {
                    change.Release();
                }
            });

            return shadow;
        }
    }
}

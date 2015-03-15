using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo.Scheduling;
using Tempo.SharedMemory;
using Tempo.Util;
using TwistedOak.Util;

namespace Tempo
{
    /// <summary>
    /// Provides the basic operators, some of which must be used in a continuous scope, others must only be used in a sequential scope.
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// Schedule an action to be performed once. This can be used to shift from a continuous scope into a sequential scope,
        /// but is also valid within an existing sequential scope.
        /// </summary>
        /// <param name="handler">The action to perform</param>
        public static void Once(Action handler)
        {
            var callingScope = CurrentThread.AnyCurrentScope();
            callingScope.ScheduleSequentialBlock(handler);
        }

        /// <summary>
        /// Schedule an action to be invoked when the current scope ends. Available from any type of scope.
        /// </summary>
        /// <param name="onScopeEnded">The action to execute when the scope ends.</param>
        public static void WhenEnded(Action onScopeEnded)
        {
            var callingScope = CurrentThread.AnyCurrentScope();

            callingScope.lifetime.WhenDead(onScopeEnded);
        }


        /// <summary>
        /// Listen for changes to a cell until the scope ends. Whenever the cell is modified, a sequential handler is scheduled.
        /// </summary>
        /// <param name="cell">The cell to observe.</param>
        /// <param name="handler">The action to execute.</param>
        public static void AnyChange(ICell cell, Action handler)
        {
            AnyChange(new ICell[] { cell }, handler);
        }

        /// <summary>
        /// Listen for changes to a collection of cells until the scope ends. Whenever any cell is modified, a sequential handler is scheduled.
        /// </summary>
        /// <param name="cells">The cells to observe.</param>
        /// <param name="handler">The action to execute when a change occurs.</param>
        public static void AnyChange(IEnumerable<ICell> cells, Action handler)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var updateable = callingScope.taskQueue.CreateUpdateable(callingScope.lifetime,
                () => CurrentThread.RunSequentialBlock(handler));

            foreach (var cell in cells)
            {
                cell.ListenForChanges(callingScope.lifetime, updateable.NeedsUpdate);
            }
        }


        /// <summary>
        /// Subscribe to an observable sequence as long as this scope is active. Each new value in the observable sequence
        /// starts a new sequential block.
        /// </summary>
        /// <typeparam name="T">The type of values in the observable sequence.</typeparam>
        /// <param name="source">The sequence to observe.</param>
        /// <param name="handler">A handler to invoke for each element in the observable sequence.</param>
        public static void Subscribe<T>(IObservable<T> source, Action<T> handler)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var subscription = source.Subscribe(new AnonymousObserver<T>(null, null, value => callingScope.ScheduleSequentialBlock(() => handler(value))));
            callingScope.lifetime.WhenDead(subscription.Dispose);
        }

        /// <summary>
        /// Observe the value produced by an observable sequence as long as the current scope is active.
        /// </summary>
        /// <typeparam name="T">The type of values in the observable sequence.</typeparam>
        /// <param name="source">The sequence of values to observe.</param>
        /// <param name="initialValue">The value that the result will hold until the first value is received from the observable sequence.</param>
        /// <returns></returns>
        public static ICellRead<T> SubscribeValue<T>(IObservable<T> source, T initialValue)
        {
            T value = initialValue;

            var result = new AnonymousCellRead<T>(
                () => value,
                (lifetime, handler) =>
                {
                    var subscription = source.Subscribe(new AnonymousObserver<T>(null, null, nextValue =>
                        {
                            value = nextValue;
                            handler();
                        }));

                    lifetime.WhenDead(subscription.Dispose);
                });

            return result;
        }

        /// <summary>
        /// Subscribe to a .NET event, conforming to the standard .NET event pattern, as long as this scope is active.
        /// Each time an event is raised, a new sequential block will be started.
        /// </summary>
        /// <param name="target">The object instance which exposes the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="handler">The sequential block to invoke for each event.</param>
        public static void Listen(object target, string eventName, Action<EventArgs> handler)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var eventHandler = new EventHandler((s, e) =>
            {
                callingScope.ScheduleSequentialBlock(() => handler(e));
            });

            var eventInfo = target.GetType().GetEvent(eventName);
            eventInfo.AddEventHandler(target, eventHandler);
            callingScope.lifetime.WhenDead(() => eventInfo.RemoveEventHandler(target, eventHandler));
        }

        /// <summary>
        /// Subscribe to a .NET event, conforming to the standard .NET event pattern, as long as this scope is active.
        /// Each time an event is raised, a new sequential block will be started.
        /// </summary>
        /// <param name="target">The object instance which exposes the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="handler">The sequential block to invoke for each event.</param>
        public static void Listen<T>(object target, string eventName, Action<T> handler) where T : EventArgs
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var eventHandler = new EventHandler<T>((s, e) =>
            {
                callingScope.ScheduleSequentialBlock(() => handler(e));
            });

            var eventInfo = target.GetType().GetEvent(eventName);
            eventInfo.AddEventHandler(target, eventHandler);
            callingScope.lifetime.WhenDead(() => eventInfo.RemoveEventHandler(target, eventHandler));
        }


        /// <summary>
        /// Subscribe to a .NET event, conforming to the standard .NET event pattern, as long as this scope is active.
        /// Each time an event is raised, a new sequential block will be started.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the event's delegate</typeparam>
        /// <typeparam name="TArgs">The type of the event arguments</typeparam>
        /// <param name="addHandler">An action to subscribe to the event.</param>
        /// <param name="removeHandler">An action to unsubscribe from the event.</param>
        /// <param name="handler">The sequential block to invoke for each event.</param>
        public static void Listen<TDelegate, TArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Action<TArgs> handler)
            where TArgs : EventArgs
            where TDelegate : class
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var innerHandler = new Action<object, TArgs>((sender, args) =>
                {
                    callingScope.ScheduleSequentialBlock(() => handler(args));
                });
            var finalHandler = DelegateUtility.Cast<TDelegate>(innerHandler);

            addHandler(finalHandler);
            callingScope.lifetime.WhenDead(() => removeHandler(finalHandler));
        }
    }
}

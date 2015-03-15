using Tempo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo
{
    /// <summary>
    /// Provides static methods for constructing observable value cells.
    /// </summary>
    public static class MemoryCellExtensions
    {
        /// <summary>
        /// Listen for changes in a cell until the scope ends. Whenever a change has occurred, a sequential handler will be
        /// invoked. If several changes occur before the handler is invoked, it will only be invoked once with the latest value.
        /// </summary>
        /// <typeparam name="T">The type of value in the cell.</typeparam>
        /// <param name="cell">The cell to observ.e</param>
        /// <param name="handler">The handler to be invoked when the cell is changed.</param>
        public static void Latest<T>(this ICellRead<T> cell, Action<T> handler)
        {
            var callingScope = CurrentThread.AnyCurrentScope();

            // The history linked list will only ever contain 0 or 1 items, since we only need the latest value
            var history = new LinkedList<T>();
            var initialValue = cell.Cur;
            RefCountHelpers.AddRef(initialValue);
            history.AddLast(initialValue);


            var updateable = callingScope.taskQueue.CreateUpdateable(callingScope.lifetime, () =>
            {
                if (history.Any())
                {
                    var historyCopy = history;
                    history = new LinkedList<T>();
                    CurrentThread.RunSequentialBlock(() =>
                    {
                        handler(historyCopy.First());
                        RefCountHelpers.ReleaseRange(historyCopy);
                    });
                }
            });

            // On each change, replace the current value in the history with the new value (updating refcounts as necessary)
            cell.ListenForChanges(callingScope.lifetime, () =>
            {
                var value = cell.Cur;
                RefCountHelpers.AddRef(value);
                if (history.Any())
                {
                    RefCountHelpers.ReleaseRange(history);
                    history.Clear();
                }
                history.AddLast(value);
                updateable.NeedsUpdate();
            });

            // When the calling scope ends, clear the history list and release any outstanding refcounts
            callingScope.lifetime.WhenDead(() =>
            {
                var historyCopy = history;
                history = new LinkedList<T>();
                RefCountHelpers.ReleaseRange(historyCopy);
            });
        }

        /// <summary>
        /// Listen for changes in a cell until the scope ends. Whenever a change has occurred, a sequential handler will be invoked.
        /// A list of all changes that have occurred are passed to the handler. Note that this may include several changes that
        /// occur within the same sequential block.
        /// </summary>
        /// <typeparam name="T">The type of value in the cell.</typeparam>
        /// <param name="cell">The cell to observe.</param>
        /// <param name="handler">The handler ot be invoked when the cell is changed.</param>
        public static void Changes<T>(this ICellRead<T> cell, Action<IEnumerable<T>> handler)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var history = new LinkedList<T>();
            var initialValue = cell.Cur;
            RefCountHelpers.AddRef(initialValue);
            history.AddLast(initialValue);

            var updateable = callingScope.taskQueue.CreateUpdateable(callingScope.lifetime, () =>
            {
                var historyCopy = history;
                history = new LinkedList<T>();
                CurrentThread.RunSequentialBlock(() =>
                {
                    handler(historyCopy);
                    RefCountHelpers.ReleaseRange(historyCopy);
                });
            });

            cell.ListenForChanges(callingScope.lifetime, () =>
            {
                var value = cell.Cur;
                RefCountHelpers.AddRef(value);
                history.AddLast(value);
                updateable.NeedsUpdate();
            });

            callingScope.lifetime.WhenDead(() =>
            {
                var historyCopy = history;
                history = new LinkedList<T>();
                RefCountHelpers.ReleaseRange(historyCopy);
            });
        }

        /// <summary>
        /// Create a scope for each value of a cell. When the value changes, the old scope is ended and a new scope is
        /// constructed. When this scope ends, the current value's scope is also ended. Each inner scope can return an
        /// ICellRead. The return value is a concatentation of all of the values returned from the inner scopes.
        /// </summary>
        /// <typeparam name="T">The type of the values of the cell.</typeparam>
        /// <typeparam name="TResult">The type of values returned from the scope constructor.</typeparam>
        /// <param name="cell">The cell to observe.</param>
        /// <param name="handler">The constructor for the inner scope.</param>
        /// <returns>A cell containing the current value of the result of the current inner scope.</returns>
        public static ICellRead<TResult> WithLatest<T, TResult>(this ICellRead<T> cell, Func<T, ICellRead<TResult>> handler)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var result = new MemoryCell<ICellRead<TResult>>(null);
            var innerLifetime = new LifetimeSource();

            Events.AnyChange(cell, () =>
            {
                innerLifetime.EndLifetime();
                innerLifetime = new LifetimeSource();

                CurrentThread.ConstructScope(callingScope, innerLifetime.Lifetime, () =>
                {
                    result.Cur = handler(cell.Cur);
                });
            });

            callingScope.lifetime.WhenDead(() =>
            {
                result.Cur = null;
                innerLifetime.EndLifetime();
            });

            return result.Flatten();
        }

        /// <summary>
        /// Create a scope for each value of a cell. When the value changes, the old scope is ended and a new scope is
        /// constructed. When this scope ends, the current value's scope is also ended. Each inner scope can return an
        /// ICellRead. The return value is a concatentation of all of the values returned from the inner scopes.
        /// If the value of the source cell is null, then the result cell holds the default value.
        /// </summary>
        /// <typeparam name="T">The type of the values of the cell.</typeparam>
        /// <typeparam name="TResult">The type of values returned from the scope constructor.</typeparam>
        /// <param name="cell">The cell to observe.</param>
        /// <param name="handler">The constructor for the inner scope.</param>
        /// <returns>A cell containing the current value of the result of the current inner scope.</returns>
        public static ICellRead<TResult> WithLatestOrDefault<T, TResult>(this ICellRead<T> cell, Func<T, ICellRead<TResult>> handler) where T : class
        {
            return WithLatest(cell, value =>
            {
                if (value == null)
                    return CellBuilder.Const(default(TResult));

                return handler(value);
            });
        }

        /// <summary>
        /// Bind the value of a cell to be equal to the value of another, until the scope ends.
        /// </summary>
        /// <typeparam name="T">The type of value in both cells.</typeparam>
        /// <param name="source">The cell to be observed.</param>
        /// <param name="destination">The cell to be constrained</param>
        public static void Bind<T>(this ICellRead<T> source, ICellWrite<T> destination)
        {
            source.Latest(value => destination.Cur = value);
        }


        /// <summary>
        /// Observes a boolean value until this scope ends: when the value is true, an inner scope is constructed. When the value
        /// becomes false, the inner scope is ended. When this scope ends, the inner scope is also ended if it is currently active.
        /// </summary>
        /// <param name="condition">The condition cell.</param>
        /// <param name="bodyConstructor">A function to construct the inner scope.</param>
        public static void WhilstTrue(this ICellRead<bool> condition, Action bodyConstructor)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();
            var bodyLifetimeSrc = new LifetimeSource();

            var lastValue = false;
            condition.Changes(history =>
            {
                foreach (var value in history)
                {
                    if (value == lastValue)
                        continue;

                    lastValue = value;

                    bodyLifetimeSrc.EndLifetime();
                    bodyLifetimeSrc = new LifetimeSource();

                    if (value)
                    {
                        CurrentThread.ConstructScope(callingScope, bodyLifetimeSrc.Lifetime, bodyConstructor);
                    }
                }
            });
        }

        /// <summary>
        /// Observes a value, applying a boolean condition: when the condition is true, an inner scope is constructed. When the condition
        /// becomes false, the inner scope is ended. When this scope ends, the inner scope is also ended if it is currently active.
        /// </summary>
        /// <param name="cell">The value cell.</param>
        /// <param name="condition">The condition to apply to cell values.</param>
        /// <param name="bodyConstructor">A function to construct the inner scope.</param>
        public static void Whilst<T>(this ICellRead<T> cell, Func<T, bool> condition, Action bodyConstructor)
        {
            cell.Select(condition).WhilstTrue(bodyConstructor);
        }


        /// <summary>
        /// Observes a boolean condition until this scope ends. Whenever the condition changes from false to true, a sequential action is invoked.
        /// </summary>
        /// <param name="condition">The condition to observe.</param>
        /// <param name="handler">The handler to invoke.</param>
        public static void WhenTrue(this ICellRead<bool> condition, Action handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            var lastValue = false;

            condition.Changes(history =>
            {
                bool shouldTrigger = false;

                foreach (var value in history)
                {
                    if (value == lastValue)
                        continue;
                    lastValue = value;

                    if (value)
                    {
                        shouldTrigger = true;
                    }
                }

                if (shouldTrigger)
                {
                    CurrentThread.RunSequentialBlock(handler);
                }
            });
        }


        public static void When<T>(this ICellRead<T> cell, Func<T, bool> condition, Action handler)
        {
            cell.Select(condition).WhenTrue(handler);
        }

        /// <summary>
        /// Construct a new observable value cell by applying a transformation to each value of another cell.
        /// </summary>
        /// <typeparam name="TIn">The type of the values in the source cell.</typeparam>
        /// <typeparam name="TOut">The type of value returned by the transform.</typeparam>
        /// <param name="source">The source cell.</param>
        /// <param name="transform">A transform function to apply to each value.</param>
        /// <returns></returns>
        public static ICellRead<TOut> Select<TIn, TOut>(this ICellRead<TIn> source, Func<TIn, TOut> transform)
        {
            return new AnonymousCellRead<TOut>(
                () => transform(source.Cur),
                source.ListenForChanges,
                null);
        }

        /// <summary>
        /// Construct a new cell which only emits a change notification if the new value is not equal to the previous value.
        /// Object.Equals is used to determine equality.
        /// </summary>
        /// <typeparam name="T">The type of values in the cell.</typeparam>
        /// <param name="source">The source cell.</param>
        /// <returns>The new cell which minimizes change notifications.</returns>
        public static ICellRead<T> DistinctUntilChanged<T>(this ICellRead<T> source)
        {
            var buffer = new MemoryCell<T>(source.Cur);

            source.Latest(value =>
                {
                    if (!Object.Equals(value, buffer.Cur))
                    {
                        buffer.Cur = value;
                    }
                });

            return buffer;
        }

        /// <summary>
        /// Flatten an observable cell of observable cells into an observable cell.
        /// </summary>
        /// <typeparam name="T">The type of values in the inner cells.</typeparam>
        /// <param name="cell">The source cell.</param>
        /// <returns>The flattened observable cell.</returns>
        public static ICellRead<T> Flatten<T>(this ICellRead<MemoryCell<T>> cell)
        {
            return cell.Select(x => (ICellRead<T>)x).Flatten();
        }

        /// <summary>
        /// Flatten an observable cell of observable cells into an observable cell.
        /// </summary>
        /// <typeparam name="T">The type of values in the inner cells.</typeparam>
        /// <param name="cell">The source cell.</param>
        /// <returns>The flattened observable cell.</returns>
        public static ICellRead<T> Flatten<T>(this ICellRead<ICellRead<T>> cell)
        {
            return new AnonymousCellRead<T>(
                () => cell.Cur == null ? default(T) : cell.Cur.Cur,
                (lifetime, handler) =>
                {
                    var innerSubLifetime = new LifetimeSource();

                    var updateInnerSubscriptions = new Action(() =>
                        {
                            innerSubLifetime.EndLifetime();
                            innerSubLifetime = new LifetimeSource();

                            var innerCell = cell.Cur;

                            if(innerCell != null)
                            {
                                innerCell.ListenForChanges(innerSubLifetime.Lifetime, handler);
                            }
                        });

                    cell.ListenForChanges(lifetime, () =>
                        {
                            updateInnerSubscriptions();
                            handler();
                        });

                    updateInnerSubscriptions();

                    lifetime.WhenDead(innerSubLifetime.EndLifetime);
                });
        }
    }
}

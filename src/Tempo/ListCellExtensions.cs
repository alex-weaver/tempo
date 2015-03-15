using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo.Util;
using TwistedOak.Util;

namespace Tempo
{
    public static class ListCellExtensions
    {
        /// <summary>
        /// Listen for changes to a list cell until the scope ends. Whenever the list is modified, a sequential handler will be
        /// invoked. The handler is passed a list of all changes that have happened. The handler is also invoked once with
        /// an 'add' change, containing the current list elements.
        /// </summary>
        /// <typeparam name="T">The type of list elements.</typeparam>
        /// <param name="source">The list to observe.</param>
        /// <param name="handler">The handler to invoke when the list changes.</param>
        public static void Changes<T>(this IListCellRead<T> source, Action<IEnumerable<ListCellChanged<T>>> handler)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var history = new LinkedList<ListCellChanged<T>>();
            if (source.Cur.Any())
            {
                var initialChange = new ListCellChanged<T>(ListChangeAction.Add, 0, source.Cur.ToList(), -1, 0);
                history.AddLast(initialChange);
            }

            var updateable = callingScope.taskQueue.CreateUpdateable(callingScope.lifetime, () =>
            {
                var historyCopy = history;
                history = new LinkedList<ListCellChanged<T>>();
                CurrentThread.RunSequentialBlock(() =>
                {
                    handler(historyCopy);
                    foreach (var item in historyCopy)
                    {
                        item.Release();
                    }
                });
            });

            source.ListenChanges(callingScope.lifetime, change =>
            {
                change.AddRef();
                history.AddLast(change);
                updateable.NeedsUpdate();
            });

            callingScope.lifetime.WhenDead(() =>
            {
                var historyCopy = history;
                history = new LinkedList<ListCellChanged<T>>();
                foreach (var item in historyCopy)
                {
                    item.Release();
                }
            });
        }


        /// <summary>
        /// Creates a new temporal scope for each element in a list cell. If new elements are added to the list, new scopes are initialized. Likewise,
        /// when an element is removed from the list its scope ends. All element scopes end when the calling temporal scope ends.
        /// This behaviour is also combined with map-like behavior, where a function is applied to each element in the list, and the resulting
        /// list is this method's return value.
        /// </summary>
        /// <typeparam name="TIn">The type of items in the source list.</typeparam>
        /// <typeparam name="TOut">The type of items in the result list.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="selector">The function to initialize each element scope.</param>
        /// <returns></returns>
        public static IListCellRead<TOut> WithEach<TIn, TOut>(this IListCellRead<TIn> source, Func<TIn, ICellRead<TOut>> selector)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            var innerLifetimes = new LifetimeList();
            var result = new ListCell<ICellRead<TOut>>();

            var constructScope = new Func<Lifetime, TIn, ICellRead<TOut>>((lifetime, input) =>
            {
                return CurrentThread.ConstructScope(callingScope, lifetime, () => selector(input));
            });

            source.Changes(changes =>
            {
                foreach (var change in changes)
                {
                    switch (change.Action)
                    {
                        case ListChangeAction.Add:
                            var newScopes_add = innerLifetimes.InsertRange(change.NewStartingIndex, change.NewItems.Count());
                            var newResults_add = newScopes_add.Zip(change.NewItems, constructScope).ToList(); // Use ToList to guarantee one iteration
                            result.InsertRange(change.NewStartingIndex, newResults_add);
                            break;
                        case ListChangeAction.Remove:
                            innerLifetimes.RemoveRange(change.OldStartingIndex, change.OldItemCount);
                            result.RemoveRange(change.OldStartingIndex, change.OldItemCount);
                            break;
                        case ListChangeAction.Replace:
                            var newScopes_replace = innerLifetimes.ReplaceRange(change.NewStartingIndex, change.NewItems.Count());
                            var newResults_replace = newScopes_replace.Zip(change.NewItems, constructScope).ToList(); // Use ToList to guarantee one iteration
                            result.ReplaceRange(change.NewStartingIndex, newResults_replace);
                            break;
                    }
                }
            });

            callingScope.lifetime.WhenDead(() => innerLifetimes.Clear());

            return result.Flatten();
        }

        /// <summary>
        /// Bind the contents of a list to be equal to the contents of a list cell, until the scope ends.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source list.</typeparam>
        /// <param name="source">The list to be observed.</param>
        /// <param name="destination">The list to be constrained.</param>
        public static void Bind<T>(this IListCellRead<T> source, System.Collections.IList destination)
        {
            destination.Clear();
            source.Changes(changes =>
            {
                foreach (var change in changes)
                {
                    switch (change.Action)
                    {
                        case ListChangeAction.Add:
                            ListRangeActions.InsertRange(destination, change.NewStartingIndex, change.NewItems);
                            break;
                        case ListChangeAction.Remove:
                            ListRangeActions.RemoveRange<T>(destination, change.OldStartingIndex, change.OldItemCount);
                            break;
                        case ListChangeAction.Replace:
                            ListRangeActions.ReplaceRange(destination, change.NewStartingIndex, change.NewItems);
                            break;
                    }
                }
            });
        }

        /// <summary>
        /// Bind the contents of a list cell to be equal to the contents of another list cell, until the scope ends.
        /// </summary>
        /// <typeparam name="T">The type of elements in the lists.</typeparam>
        /// <param name="source">The list to be observed.</param>
        /// <param name="destination">The list to be constrained.</param>
        public static void Bind<T>(this IListCellRead<T> source, IListCellWrite<T> destination)
        {
            destination.Clear();
            source.Changes(changes =>
            {
                foreach (var change in changes)
                {
                    switch (change.Action)
                    {
                        case ListChangeAction.Add:
                            destination.InsertRange(change.NewStartingIndex, change.NewItems);
                            break;
                        case ListChangeAction.Remove:
                            destination.RemoveRange(change.OldStartingIndex, change.OldItemCount);
                            break;
                        case ListChangeAction.Replace:
                            destination.ReplaceRange(change.NewStartingIndex, change.NewItems);
                            break;
                    }
                }
            });
        }


        /// <summary>
        /// Projects each element of a list cell by applying a transformation function.
        /// </summary>
        /// <typeparam name="TIn">The type of the elements of the source list.</typeparam>
        /// <typeparam name="TOut">The type of value returned by transform.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="transform">A transform function to apply to each element.</param>
        /// <returns>A read-only list cell whose elements are the result of invoking the transform function on each element of source.</returns>
        public static IListCellRead<TOut> Select<TIn, TOut>(this IListCellRead<TIn> source, Func<TIn, TOut> transform)
        {
            return new AnonymousListCellRead<TOut>(
                () => source.Cur.Select(transform),
                source.ListenForChanges,
                (lifetime, handler) => source.ListenChanges(lifetime, changes => handler(TransformChanges(changes, transform))));
        }

        private static ListCellChanged<TOut> TransformChanges<TIn, TOut>(ListCellChanged<TIn> source, Func<TIn, TOut> transform)
        {
            return new ListCellChanged<TOut>(
                source.Action,
                source.NewStartingIndex,
                source.NewItems == null ? null : source.NewItems.Select(transform).ToList(),
                source.OldStartingIndex,
                source.OldItemCount);
        }

        /// <summary>
        /// Returns a read-only memory cell which is always equal to the first element of a list.
        /// </summary>
        /// <typeparam name="T">The type of elements of list.</typeparam>
        /// <param name="list">The source list.</param>
        /// <returns>A cell which always contains the first element of list, or the default value of T if the list is empty.</returns>
        public static ICellRead<T> FirstOrDefault<T>(this IListCellRead<T> list)
        {
            return new AnonymousCellRead<T>(
                () => list.Cur.FirstOrDefault(),
                (lifetime, handler) => list.ListenForChanges(lifetime, handler));
        }

        /// <summary>
        /// Returns an observable value containing the element of an observable list at a specified observable index.
        /// </summary>
        /// <typeparam name="T">The type of element in the list</typeparam>
        /// <param name="list">The observable list.</param>
        /// <param name="index">The observable value containing the index into the list.</param>
        /// <returns></returns>
        public static ICellRead<T> ElementAt<T>(this IListCellRead<T> list, ICellRead<int> index)
        {
            return new AnonymousCellRead<T>(
                () => index.Cur >= 0 && index.Cur < list.Cur.Count() ? list.Cur.ElementAt(index.Cur) : default(T),
                (lifetime, handler) =>
                {
                    list.ListenForChanges(lifetime, handler);
                    index.ListenForChanges(lifetime, handler);
                });
        }

        public static ICellRead<TResult> Aggregate<T, TResult>(this IListCellRead<T> list, Func<IEnumerable<T>, TResult> aggregator)
        {
            return new AnonymousCellRead<TResult>(
                () => aggregator(list.Cur),
                list.ListenForChanges);
        }


        /// <summary>
        /// Flatten a list of observable values into a list. If an observable element changes value, the result
        /// list emits a 'replace' change.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="list">The list of observable values.</param>
        /// <returns>The flattened list.</returns>
        public static IListCellRead<T> Flatten<T>(this IListCellRead<ICellRead<T>> list)
        {
            return list.Flatten<T, ICellRead<T>>();
        }

        /// <summary>
        /// Flatten a list of observable values into a list. If an observable element changes value, the result
        /// list emits a 'replace' change.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="list">The list of observable values.</param>
        /// <returns>The flattened list.</returns>
        public static IListCellRead<T> Flatten<T>(this IListCellRead<MemoryCell<T>> list)
        {
            return list.Flatten<T, MemoryCell<T>>();
        }

        /// <summary>
        /// Flatten a list of observable values into a list. If an observable element changes value, the result
        /// list emits a 'replace' change.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="list">The list of observable values.</param>
        /// <returns>The flattened list.</returns>
        public static IListCellRead<T> Flatten<T, TCell>(this IListCellRead<TCell> list) where TCell : ICellRead<T>
        {
            return new AnonymousListCellRead<T>(
                () => list.Cur.Select(x => x == null ? default(T) : x.Cur),
                (lifetime, handler) =>
                {
                    list.ListenForChanges(lifetime, handler);

                    var lifetimes = new List<LifetimeSource>();
                    list.ListenChanges(lifetime, change =>
                        {
                            switch (change.Action)
                            {
                                case ListChangeAction.Add:
                                    AddDependencies(change.NewItems, lifetimes, index => handler());
                                    break;
                                case ListChangeAction.Remove:
                                    RemoveDependencies(change.OldStartingIndex, change.OldItemCount, lifetimes);
                                    break;
                                case ListChangeAction.Replace:
                                    RemoveDependencies(change.OldStartingIndex, change.OldItemCount, lifetimes);
                                    AddDependencies(change.NewItems, lifetimes, index => handler());
                                    break;
                                    
                            }
                        });
                },
                (lifetime, handler) =>
                {
                    var innerHandlerSrc = new LifetimeSource();
                    list.ListenChanges(lifetime, change =>
                        {
                            innerHandlerSrc.EndLifetime();
                            innerHandlerSrc = new LifetimeSource();

                            int i = 0;
                            foreach (var item in list.Cur)
                            {
                                int indexCopy = i;
                                item.ListenForChanges(innerHandlerSrc.Lifetime, () =>
                                    {
                                        var container = list.Cur.ElementAt(indexCopy);
                                        var element = container == null ? default(T) : container.Cur;

                                        var dummyChange = new ListCellChanged<T>(ListChangeAction.Replace,
                                            indexCopy, Enumerable.Repeat(element, 1).ToList(),
                                            indexCopy, 1);
                                        handler(dummyChange);
                                        dummyChange.Release();
                                    });
                                ++i;
                            }

                            handler(TransformChanges(change, x => x == null ? default(T) : x.Cur));
                        });
                });
        }

        private static void AddDependencies<T>(IEnumerable<T> newDeps, List<LifetimeSource> lifetimes, Action<int> handler) where T : ICell
        {
            int i = 0;
            foreach (var dep in newDeps)
            {
                int indexCopy = i;
                var depLifetimeSrc = new LifetimeSource();
                dep.ListenForChanges(depLifetimeSrc.Lifetime, () => handler(indexCopy));
                lifetimes.Insert(i, depLifetimeSrc);
                ++i;
            }
        }

        private static void RemoveDependencies(int index, int count, List<LifetimeSource> lifetimes)
        {
            for (int j = 0; j < count; ++j)
            {
                lifetimes[index].EndLifetime();
                lifetimes.RemoveAt(index);
            }
        }
    }
}

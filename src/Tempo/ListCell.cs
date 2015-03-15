using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TwistedOak.Util;
using Tempo.Util;

namespace Tempo
{
    /// <summary>
    /// Provides a list cell which can be observed for changes.
    /// </summary>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    public class ListCell<T> : RefCountedSafe, IListCellRead<T>, IListCellWrite<T>
    {
        private List<T> _items = new List<T>();
        private MessageRelay<ListCellChanged<T>> _changes = new MessageRelay<ListCellChanged<T>>();


        /// <summary>
        /// Construct a new list cell. The scope defines the lifetime of the list. When the lifetime ends,
        /// any reference counted elements are released.
        /// </summary>
        /// <param name="scope">The scope defining the lifetime of the list.</param>
        public ListCell()
        {
            var scope = CurrentThread.AnyCurrentScope();
            scope.lifetime.WhenDead(Release);
        }

        protected override void Destroy()
        {
            // Force reference counted items to be released
            // Don't use Clear() here to prevent notifications from being raised
            RefCountHelpers.ReleaseRange(_items);
            _items.Clear();
        }

        /// <summary>
        /// Observe the list cell for changes. This is an internal implementation method, and should not be used unless writing
        /// an extension to the core library.
        /// </summary>
        /// <remarks>
        /// If any list elements in the ListCellChanged object are reference counted, they will be released as soon as the handler
        /// returns. If the handler retains a reference to any of the elemens, it should call AddRef on each of them.
        /// </remarks>
        /// <param name="lifetime">The lifetime of the subscription.</param>
        /// <param name="handler">The handler to invoke on each change.</param>
        public void ListenChanges(Lifetime lifetime, Action<ListCellChanged<T>> handler)
        {
            _changes.AddHandler(lifetime, value =>
                {
                    handler(value);
                });
        }

        /// <summary>
        /// Observe the list cell for changes. This is an internal implementation method, and should not be used unless writing
        /// an extension to the core library.
        /// </summary>
        /// <param name="lifetime">The lifetime of the subscription.</param>
        /// <param name="handler">The handler to invoke on each change.</param>
        public void ListenForChanges(Lifetime lifetime, Action handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            _changes.AddHandler(lifetime, value => handler());
        }

        /// <summary>
        /// Gets the current elements of the list cell.
        /// </summary>
        public IEnumerable<T> Cur
        {
            get { return _items; }
        }

        /// <summary>
        /// Gets the count of elements of the list cell.
        /// </summary>
        public int Count { get { return _items.Count; } }

        /// <summary>
        /// Gets or replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or replace.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return _items[index]; }
            set
            {
                this.ReplaceRange(index, Enumerable.Repeat(value, 1));
            }
        }

        /// <summary>
        /// Adds an item to the end of the list.
        /// </summary>
        /// <param name="value">The item to add.</param>
        /// <returns>The index of the item added.</returns>
        public int Add(T value)
        {
            var newIndex = _items.Count;
            var change = new ListCellChanged<T>(ListChangeAction.Add, newIndex, Enumerable.Repeat<T>(value, 1).ToList(), -1, 0);
            _items.Add(value);
            RefCountHelpers.AddRef(value);
            _changes.Broadcast(change);
            change.Release();
            return newIndex;
        }

        /// <summary>
        /// Adds the elements in the specified collection to the end of the list.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            InsertRange(_items.Count, items);
        }

        /// <summary>
        /// Inserts the elements of a collection at the specified index.
        /// </summary>
        /// <param name="index">The index to insert the elements at.</param>
        /// <param name="items">The items to insert.</param>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            var itemsCopy = items.ToList(); // change requires IList not IEnumerable, also this guarantees items is only traversed once

            var change = new ListCellChanged<T>(ListChangeAction.Add, index, itemsCopy, -1, 0);
            _items.InsertRange(index, itemsCopy);
            RefCountHelpers.AddRefRange(itemsCopy);
            _changes.Broadcast(change);
            change.Release();
        }


        /// <summary>
        /// Remove the given element from the list.
        /// </summary>
        /// <param name="value">The element to remove.</param>
        public void Remove(T value)
        {
            var index = _items.IndexOf(value);
            if(index >= 0 && index < _items.Count)
            {
                RemoveAt(index);
            }
        }

        /// <summary>
        /// Remove the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            RemoveRange(index, 1);
        }

        /// <summary>
        /// Remove all elements matching the specified predicate.
        /// </summary>
        /// <param name="condition">The predicate to match the elements to remove.</param>
        public void RemoveAll(Predicate<T> condition)
        {
            for(int i = 0; i < _items.Count; ++i)
            {
                if (condition(_items[i]))
                {
                    var toRemove = _items[i];
                    var change = new ListCellChanged<T>(ListChangeAction.Remove, -1, null, i, 1);
                    _items.RemoveAt(i);
                    RefCountHelpers.Release(toRemove);
                    _changes.Broadcast(change);
                    change.Release();
                    --i;
                }
            }
        }

        /// <summary>
        /// Remove the elements at the specified range of indices.
        /// </summary>
        /// <param name="index">The starting index of the range to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        public void RemoveRange(int index, int count)
        {
            if (index < 0 || (index + count) > _items.Count)
                throw new ArgumentOutOfRangeException("index and count must describe a range inside the bounds of the collection");

            var change = new ListCellChanged<T>(ListChangeAction.Remove, -1, null, index, count);
            RefCountHelpers.ReleaseRange(_items.Skip(index).Take(count)); // Could potentially optimize away by not using AddRefRange for ListCellChange constructor. Would obfuscate code.
            _items.RemoveRange(index, count);
            _changes.Broadcast(change);
            change.Release();
        }

        /// <summary>
        /// Replace the elements at the specified index with a collection of new items.
        /// </summary>
        /// <param name="index">The index to start replacing.</param>
        /// <param name="newItems">The new items.</param>
        public void ReplaceRange(int index, IEnumerable<T> newItems)
        {
            var newItemsCopy = newItems.ToList(); // change requires IList not IEnumerable, also this guarantees items is only traversed once

            RefCountHelpers.AddRefRange(newItemsCopy);
            var count = newItemsCopy.Count();
            var change = new ListCellChanged<T>(ListChangeAction.Replace, index, newItemsCopy, index, count);

            var oldItems = _items.Skip(index).Take(count).ToList();
            RefCountHelpers.ReleaseRange(oldItems);

            var i = index;
            foreach (var item in newItemsCopy)
            {
                _items[i] = item;
                ++i;
            }
            _changes.Broadcast(change);
            change.Release();
        }

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        public void Clear()
        {
            if (_items.Any())
            {
                var change = new ListCellChanged<T>(ListChangeAction.Remove, -1, null, 0, _items.Count);

                RefCountHelpers.ReleaseRange(_items);

                _items = new List<T>();
                _changes.Broadcast(change);
                change.Release();
            }
        }

        
        /// <summary>
        /// Determines whether there exists any element matching the specified predicate.
        /// </summary>
        /// <param name="match">The predicate that defines the conditions of the elements to search for.</param>
        /// <returns>True if one or more elements match the specified predicate, false otherwise.</returns>
        public bool Exists(Predicate<T> match)
        {
            return _items.Exists(match);
        }

        /// <summary>
        /// Searches for an element that matches the specified predicate, and returns the first element that matches.
        /// </summary>
        /// <param name="match">The predicate that elements are matched against.</param>
        /// <returns>The first element matching the predicate; otherwise, the default value for type T</returns>
        public T Find(Predicate<T> match)
        {
            return _items.Find(match);
        }

        /// <summary>
        /// Searches for an element that matches the specified predicate, and returns the index of first element that matches.
        /// </summary>
        /// <param name="match">The predicate that elements are matched against.</param>
        /// <returns>The index of first element matching the predicate; otherwise, -1</returns>
        public int FindIndex(Predicate<T> match)
        {
            return _items.FindIndex(match);
        }
    }
}

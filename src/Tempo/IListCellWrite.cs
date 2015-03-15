using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    /// <summary>
    /// Exposes a write-only interface for a list cell
    /// </summary>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    public interface IListCellWrite<T>
    {
        /// <summary>
        /// Replaces the value at the given index with a new value.
        /// </summary>
        /// <param name="index"></param>
        T this[int index] { set; }

        /// <summary>
        /// Adds an item to the end of the list.
        /// </summary>
        /// <param name="value">The item to add.</param>
        /// <returns>The index of the item added.</returns>
        int Add(T value);

        /// <summary>
        /// Adds the elements in the specified collection to the end of the list.
        /// </summary>
        /// <param name="items">The items to add.</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Inserts the elements of a collection at the specified index.
        /// </summary>
        /// <param name="index">The index to insert the elements at.</param>
        /// <param name="items">The items to insert.</param>
        void InsertRange(int index, IEnumerable<T> items);

        /// <summary>
        /// Remove the given element from the list.
        /// </summary>
        /// <param name="value">The element to remove.</param>
        void Remove(T value);

        /// <summary>
        /// Remove the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Remove all elements matching the specified predicate.
        /// </summary>
        /// <param name="condition">The predicate to match the elements to remove.</param>
        void RemoveAll(Predicate<T> condition);

        /// <summary>
        /// Remove the elements at the specified range of indices.
        /// </summary>
        /// <param name="index">The starting index of the range to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        void RemoveRange(int index, int count);

        /// <summary>
        /// Replace the elements at the specified index with a collection of new items.
        /// </summary>
        /// <param name="index">The index to start replacing.</param>
        /// <param name="newItems">The new items.</param>
        void ReplaceRange(int index, IEnumerable<T> newItems);

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        void Clear();
    }
}

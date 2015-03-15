using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo
{
    /// <summary>
    /// Exposes a read-only interface for a list cell
    /// </summary>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    public interface IListCellRead<T> : IRefCounted, ICell
    {
        /// <summary>
        /// Gets the current elements of the list cell.
        /// </summary>
        IEnumerable<T> Cur { get; }

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
        void ListenChanges(Lifetime lifetime, Action<ListCellChanged<T>> handler);

        /// <summary>
        /// Gets the count of elements of the list cell.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns></returns>
        T this[int index] { get; }

        /// <summary>
        /// Determines whether there exists any element matching the specified predicate.
        /// </summary>
        /// <param name="match">The predicate that defines the conditions of the elements to search for.</param>
        /// <returns>True if one or more elements match the specified predicate, false otherwise.</returns>
        bool Exists(Predicate<T> match);

        /// <summary>
        /// Searches for an element that matches the specified predicate, and returns the first element that matches.
        /// </summary>
        /// <param name="match">The predicate that elements are matched against.</param>
        /// <returns>The first element matching the predicate; otherwise, the default value for type T</returns>
        T Find(Predicate<T> match);

        /// <summary>
        /// Searches for an element that matches the specified predicate, and returns the index of first element that matches.
        /// </summary>
        /// <param name="match">The predicate that elements are matched against.</param>
        /// <returns>The index of first element matching the predicate; otherwise, -1</returns>
        int FindIndex(Predicate<T> match);

    }
}

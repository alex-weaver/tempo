using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo.Util
{
    /// <summary>
    /// Represents a read-only list cell.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class AnonymousListCellRead<T> : RefCountedSafe, IListCellRead<T>
    {
        private Func<IEnumerable<T>> getCur;
        private Action<Lifetime, Action> listen;
        private Action<Lifetime, Action<ListCellChanged<T>>> listenChanges;
        private Action destroy;


        /// <summary>
        /// Construct a new read-only list cell.
        /// </summary>
        /// <param name="getCur">A function to compute the current contents of the list.</param>
        /// <param name="listen">A function to register a listener.</param>
        /// <param name="listenChanges">A function to register a listener for specific list changes.</param>
        public AnonymousListCellRead(Func<IEnumerable<T>> getCur, Action<Lifetime, Action> listen, Action<Lifetime, Action<ListCellChanged<T>>> listenChanges)
            : this(getCur, listen, listenChanges, null)
        {
        }

        /// <summary>
        /// Construct a new read-only list cell.
        /// </summary>
        /// <param name="getCur">A function to compute the current contents of the list.</param>
        /// <param name="listen">A function to register a listener.</param>
        /// <param name="listenChanges">A function to register a listener for specific list changes.</param>
        /// <param name="destroy">An action to handle cleanup after all references have been released. May be null if no cleanup is required.</param>
        public AnonymousListCellRead(Func<IEnumerable<T>> getCur, Action<Lifetime, Action> listen, Action<Lifetime, Action<ListCellChanged<T>>> listenChanges,
            Action destroy)
        {
            if(getCur == null) throw new ArgumentNullException("getCur");
            if(listen == null) throw new ArgumentNullException("listen");
            if(listenChanges == null) throw new ArgumentNullException("listenChanges");

            this.getCur = getCur;
            this.listen = listen;
            this.listenChanges = listenChanges;
            this.destroy = destroy;
        }

        protected override void Destroy()
        {
            if(destroy != null)
            {
                destroy();
            }
        }

        /// <summary>
        /// Gets the current elements of the list cell.
        /// </summary>
        public IEnumerable<T> Cur
        {
            get { return getCur(); }
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
            listenChanges(lifetime, handler);
        }

        /// <summary>
        /// Subscribe to notifications that the cell has changed. The listener is unsubscribed when the lifetime ends.
        /// </summary>
        /// <param name="lifetime">The lifetime of the subscription.</param>
        /// <param name="handler">The action to invoke when a change occurs.</param>
        public void ListenForChanges(Lifetime lifetime, Action handler)
        {
            listen(lifetime, handler);
        }

        /// <summary>
        /// Gets the count of elements of the list cell.
        /// </summary>
        public int Count
        {
            get { return Cur.Count(); }
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return Cur.ElementAt(index); }
        }

        /// <summary>
        /// Determines whether there exists any element matching the specified predicate.
        /// </summary>
        /// <param name="match">The predicate that defines the conditions of the elements to search for.</param>
        /// <returns>True if one or more elements match the specified predicate, false otherwise.</returns>
        public bool Exists(Predicate<T> match)
        {
            return Cur.Any(x => match(x));
        }

        /// <summary>
        /// Searches for an element that matches the specified predicate, and returns the first element that matches.
        /// </summary>
        /// <param name="match">The predicate that elements are matched against.</param>
        /// <returns>The first element matching the predicate; otherwise, the default value for type T</returns>
        public T Find(Predicate<T> match)
        {
            return Cur.FirstOrDefault(x => match(x));
        }

        /// <summary>
        /// Searches for an element that matches the specified predicate, and returns the index of first element that matches.
        /// </summary>
        /// <param name="match">The predicate that elements are matched against.</param>
        /// <returns>The index of first element matching the predicate; otherwise, -1</returns>
        public int FindIndex(Predicate<T> match)
        {
            int index = 0;
            foreach (var item in Cur)
            {
                if (match(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}

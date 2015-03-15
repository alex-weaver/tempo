using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo.Util
{
    /// <summary>
    /// Represents a read-only cell.
    /// </summary>
    /// <typeparam name="T">The type of value in the cell.</typeparam>
    public class AnonymousCellRead<T> : RefCountedSafe, ICellRead<T>
    {
        private Func<T> getCur;
        private Action<Lifetime, Action> listen;
        private Action destroy;


        /// <summary>
        /// Constructs a new read-only cell, given a function to compute the current cell value, and a function to
        /// register a listener.
        /// </summary>
        /// <param name="getCur">A function to compute the current value of the cell. May not be null.</param>
        /// <param name="listen">A function to register a listener. May not be null.</param>
        public AnonymousCellRead(Func<T> getCur, Action<Lifetime, Action> listen)
            : this(getCur, listen, null)
        {
        }

        /// <summary>
        /// Constructs a new read-only cell, given a function to compute the current cell value, and a function to
        /// register a listener.
        /// </summary>
        /// <param name="getCur">A function to compute the current value of the cell. May not be null.</param>
        /// <param name="listen">A function to register a listener. May not be null.</param>
        /// <param name="destroy">An action to handle cleanup after all references have been released. May be null if no cleanup is required.</param>
        public AnonymousCellRead(Func<T> getCur, Action<Lifetime, Action> listen, Action destroy)
        {
            if (getCur == null) throw new ArgumentNullException("getCur");
            if (listen == null) throw new ArgumentNullException("listen");

            this.getCur = getCur;
            this.listen = listen;
            this.destroy = destroy;
        }

        protected override void Destroy()
        {
            if (destroy != null)
            {
                destroy();
            }
        }

        /// <summary>
        /// Gets the current value of the cell.
        /// </summary>
        public T Cur
        {
            get { return getCur(); }
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
    }
}

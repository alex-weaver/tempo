using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;
using Tempo.Util;

namespace Tempo
{
    /// <summary>
    /// Exposes a read-only interface for an observable value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface ICellRead<T> : ICell, IRefCounted
    {
        /// <summary>
        /// Get the current value of the cell.
        /// </summary>
        T Cur { get; }
    }

    /// <summary>
    /// Exposes a write-only interface for an observable value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface ICellWrite<T>
    {
        /// <summary>
        /// Set the value of the cell.
        /// </summary>
        T Cur { set; }

        /// <summary>
        /// Set the value of the cell.
        /// </summary>
        /// <param name="value">The new value.</param>
        void Set(T value);
    }

    /// <summary>
    /// Represents an observable cell. If reference counted values are put in the cell, the cell will retain a reference.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class MemoryCell<T> : RefCountedSafe, ICellRead<T>, ICellWrite<T>
    {
        private T _currentValue;
		private readonly MessageRelay<Unit> _changes = new MessageRelay<Unit>();
        private bool isActive = true;


        /// <summary>
        /// Constructs a new memory cell, with a lifetime defined by the given scope. When the scope ends,
        /// if the cell contains a reference counted value, it is released.
        /// </summary>
        /// <param name="scope">The lifetime of the cell.</param>
        /// <param name="initialValue">The initial value to store in the cell.</param>
		public MemoryCell(T initialValue)
		{
            var scope = CurrentThread.AnyCurrentScope();
            scope.lifetime.WhenDead(Release);

            RefCountHelpers.AddRef(initialValue);
			_currentValue = initialValue;
		}

        protected override void Destroy()
        {
            // Don't use Set() here to prevent notifications from being raised
            RefCountHelpers.Release(_currentValue);
            _currentValue = default(T);
            isActive = false;
        }


        /// <summary>
        /// Subscribe to notifications that the cell has changed. The listener is unsubscribed when the lifetime ends.
        /// This is an internal method, which should only be used if extending the core library.
        /// </summary>
        /// <param name="lifetime">The lifetime of the subscription.</param>
        /// <param name="handler">The action to invoke when a change occurs.</param>
        public void ListenForChanges(Lifetime lifetime, Action handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            _changes.AddHandler(lifetime, unit => handler());
        }


        /// <summary>
        /// Assign a new value to the cell. If the value is reference counted, AddRef() will be called before
        /// this method returns.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void Set(T value)
        {
            if (!isActive)
                return;

            if (!Object.Equals(value, _currentValue))
            {
                var oldValue = _currentValue;

                RefCountHelpers.AddRef(value);

                _currentValue = value;
                _changes.Broadcast(Unit.Value);

                RefCountHelpers.Release(oldValue);
            }
        }

        /// <summary>
        /// Get or set the current value of the cell.
        /// </summary>
		public T Cur
		{
			get { return _currentValue; }
			set
			{
                Set(value);
			}
		}
    }
}

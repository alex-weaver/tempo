using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    /// <summary>
    /// Provides a basic implementation of IRefCounted, which is not thread safe. Inheritors need only implement the Destroy() method,
    /// which will be called when the reference count reaches zero. The reference count automatically starts at 1.
    /// </summary>
    public abstract class RefCountedUnsafe : IRefCounted
    {
        private int refCount = 1;

        /// <summary>
        /// Construct a new reference counted object. The reference count automatically starts at 1.
        /// </summary>
        public RefCountedUnsafe()
        {
        }

        /// <summary>
        /// This method is called when the reference count reaches zero. It is guaranteed to be called at most once.
        /// </summary>
        protected abstract void Destroy();

        /// <summary>
        /// Returns true if the object has already been destroyed.
        /// </summary>
        /// <returns>true if the object is destroyed; false otherwise.</returns>
        protected bool IsDestroyed()
        {
            return refCount == 0;
        }

        /// <summary>
        /// Throws an exception if the object has been destroyed, otherwise does nothing.
        /// </summary>
        protected void ThrowIfDestroyed()
        {
            if (IsDestroyed())
            {
                throw new InvalidOperationException("Reference counted object has been destroyed");
            }
        }

        /// <summary>
        /// Increments the reference count. If the object has been destroyed, an exception is thrown.
        /// </summary>
        public void AddRef()
        {
            if (IsDestroyed()) throw new InvalidOperationException("Cannot AddRef - object has already been destroyed");

            ++refCount;
        }

        /// <summary>
        /// Decrements the reference count. If the reference count reaches zero, the object is destroyed.
        /// If the object has been destroyed, an exception is thrown.
        /// </summary>
        public void Release()
        {
            if (IsDestroyed()) throw new InvalidOperationException("Cannot release object - object has already been destroyed");

            --refCount;

            if (refCount <= 0)
                Destroy();
        }
    }
}

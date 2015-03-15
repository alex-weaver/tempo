using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tempo
{
    /// <summary>
    /// Provides a thread safe implementation of IRefCounted. Inheritors need only implement the Destroy() method,
    /// which will be called when the reference count reaches zero. The reference count automatically starts at 1.
    /// </summary>
    public abstract class RefCountedSafe : IRefCounted
    {
        private int refCount = 1;

        /// <summary>
        /// Construct a new reference counted object. The reference count automatically starts at 1.
        /// </summary>
        public RefCountedSafe()
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
            int curRefCount;
            int original;
            var spin = new SpinWait();
            while (true)
            {
                curRefCount = refCount;
                if (curRefCount == 0) throw new InvalidOperationException("Cannot AddRef - object has already been destroyed");
                var desiredRefCount = curRefCount + 1;
                original = Interlocked.CompareExchange(ref refCount, desiredRefCount, curRefCount);
                if (original == curRefCount) break;
                spin.SpinOnce();
            }
        }

        /// <summary>
        /// Decrements the reference count. If the reference count reaches zero, the object is destroyed.
        /// If the object has been destroyed, an exception is thrown.
        /// </summary>
        public void Release()
        {
            int original;
            int curRefCount;
            var spin = new SpinWait();
            while(true)
            {
                curRefCount = refCount;
                if (curRefCount == 0) throw new InvalidOperationException("Cannot release object - object has already been destroyed");
                var desiredRefCount = refCount - 1;
                original = Interlocked.CompareExchange(ref refCount, desiredRefCount, curRefCount);
                if (original == curRefCount) break;
                spin.SpinOnce();
            }

            if (refCount <= 0)
                Destroy();
        }
    }
}

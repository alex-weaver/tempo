using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    /// <summary>
    /// The base interface for all reference counted values. If a value requires manual disposal, it should
    /// implement this interface. Cells will automatically call AddRef and Release if they retain a reference
    /// to a value implementing this interface.
    /// </summary>
    public interface IRefCounted
    {
        /// <summary>
        /// Increment the reference count. If the object has already been destroyed, calling this method throws an exception.
        /// </summary>
        void AddRef();

        /// <summary>
        /// Decrement the reference count. If the count reaches zero, the object is destroyed.
        /// If the object has already been destroyed, calling this method throws an exception.
        /// </summary>
        void Release();
    }
}

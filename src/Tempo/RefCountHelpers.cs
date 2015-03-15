using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    /// <summary>
    /// Provides a number of static methods to work with reference counted values.
    /// </summary>
    public static class RefCountHelpers
    {
        /// <summary>
        /// Returns true if the type T implements IRefCounted.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <returns>True if T implements IRefCounted; false otherwise.</returns>
        private static bool IsRefCounted<T>()
        {
            return typeof(IRefCounted).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Increments the reference count on an object if it implements IRefCounted.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The target object.</param>
        public static void AddRef<T>(T obj)
        {
            if (obj != null && IsRefCounted<T>())
            {
                ((IRefCounted)obj).AddRef();
            }
        }

        /// <summary>
        /// Decrements the reference count on an object if it implements IRefCounted.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The target object.</param>
        public static void Release<T>(T obj)
        {
            if (obj != null && IsRefCounted<T>())
            {
                ((IRefCounted)obj).Release();
            }
        }

        /// <summary>
        /// Increments the reference count on a collection of objects, if the objects implement IRefCounted.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="items">The target collection.</param>
        public static void AddRefRange<T>(IEnumerable<T> items)
        {
            if (items != null && IsRefCounted<T>())
            {
                foreach (var item in items)
                {
                    if (item != null)
                    {
                        ((IRefCounted)item).AddRef();
                    }
                }
            }
        }

        /// <summary>
        /// Decrements the reference count on a collection of objects, if the objects implement IRefCounted.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="items">The target collection.</param>
        public static void ReleaseRange<T>(IEnumerable<T> items)
        {
            if (items != null && IsRefCounted<T>())
            {
                foreach (var item in items)
                {
                    if (item != null)
                    {
                        ((IRefCounted)item).Release();
                    }
                }
            }
        }
    }
}

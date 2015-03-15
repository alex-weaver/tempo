using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    /// <summary>
    /// A collection of enumerable extensions that are useful in combination with the other features of the library.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the maximum value of an element in the source IEnumerable, where the order is based on a key derived from
        /// the collection elements.
        /// </summary>
        /// <typeparam name="T">The type of collection element.</typeparam>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="selector">A function to derive the key from an element.</param>
        /// <returns></returns>
        public static T MaxObject<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
          where TKey : IComparable<TKey>
        {
            if (source == null) throw new ArgumentNullException("source");
            bool first = true;
            T maxObj = default(T);
            TKey maxKey = default(TKey);
            foreach (var item in source)
            {
                if (first)
                {
                    maxObj = item;
                    maxKey = selector(maxObj);
                    first = false;
                }
                else
                {
                    TKey currentKey = selector(item);
                    if (currentKey.CompareTo(maxKey) > 0)
                    {
                        maxKey = currentKey;
                        maxObj = item;
                    }
                }
            }
            if (first) throw new InvalidOperationException("Sequence is empty.");
            return maxObj;
        }
    }
}

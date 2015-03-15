using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo.Util
{
    /// <summary>
    /// A group of static methods to implement range operations for IList.
    /// </summary>
    public static class ListRangeActions
    {
        /// <summary>
        /// Inserts a range of items into a specified list.
        /// </summary>
        /// <typeparam name="T">The type of element in the items collection.</typeparam>
        /// <param name="destination">The list to insert the new elements into.</param>
        /// <param name="index">The index at which to start inserting items.</param>
        /// <param name="items">The items to insert.</param>
        public static void InsertRange<T>(System.Collections.IList destination, int index, IEnumerable<T> items)
        {
            if (destination is List<T>)
            {
                var list = (List<T>)destination;
                list.InsertRange(index, items);
            }
            else
            {
                int i = index;
                foreach (var item in items)
                {
                    destination.Insert(i, item);
                    ++i;
                }
            }
        }


        /// <summary>
        /// Removes a range of items from a specified IList.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="target">The list to remove items from.</param>
        /// <param name="startIndex">The index at which to start removing elements.</param>
        /// <param name="count">The number of items to remove.</param>
        public static void RemoveRange<T>(System.Collections.IList target, int startIndex, int count)
        {
            if (target is List<T>)
            {
                var list = (List<T>)target;
                list.RemoveRange(startIndex, count);
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    target.RemoveAt(startIndex);
                }
            }
        }

        /// <summary>
        /// Replaces a range of items in a specified IList.
        /// </summary>
        /// <typeparam name="T">The type of element in the items collection.</typeparam>
        /// <param name="destination">The list in which to replace elements.</param>
        /// <param name="index">The index at which to start replacing items.</param>
        /// <param name="newItems">The collection of new items.</param>
        public static void ReplaceRange<T>(System.Collections.IList destination, int index, IEnumerable<T> newItems)
        {
            int i = index;
            foreach (var item in newItems)
            {
                destination[i] = item;
                ++i;
            }
        }
    }
}

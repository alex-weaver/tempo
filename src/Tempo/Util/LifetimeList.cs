using Tempo.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo.Util
{
    /// <summary>
    /// Maintains a list of lifetimes with range operations. This is used by the WithEach() to track inner scope lifetimes.
    /// The LifetimeList constructs new lifetimes when they are added, and ends them when they are removed.
    /// </summary>
    public class LifetimeList
    {
        private List<LifetimeSource> _sources = new List<LifetimeSource>();


        /// <summary>
        /// Construct a new lifetime list.
        /// </summary>
        public LifetimeList()
        {
        }

        /// <summary>
        /// End and remove all inner scopes
        /// </summary>
        public void Clear()
        {
            RemoveRange(0, _sources.Count);
        }

        /// <summary>
        /// Construct one or more lifetimes, insert them into the list and return them.
        /// </summary>
        /// <param name="index">The starting index to begin inserting the new lifetimes.</param>
        /// <param name="count">The number of lifetimes to construct and insert.</param>
        /// <returns>A collection containing the new lifetimes.</returns>
        public IEnumerable<Lifetime> InsertRange(int index, int count)
        {
            // Make sure items is only iterated once (Select clause is side-effecting)
            var items = Enumerable
                .Range(0, count)
                .Select(x => new LifetimeSource());
            _sources.InsertRange(index, items);

            // Create a sepearate enumerator here so that items is only iterated once
            return _sources.Skip(index).Take(count).Select(x => x.Lifetime);
        }


        /// <summary>
        /// Remove one or more lifetimes from the list, and end each of them.
        /// </summary>
        /// <param name="index">The index at which to start removing lifetimes.</param>
        /// <param name="count">The number of lifetimes to remove and end.</param>
        public void RemoveRange(int index, int count)
        {
            for(int i = index; i < index + count; ++i)
            {
                _sources[i].EndLifetime();
            }
            _sources.RemoveRange(index, count);
        }


        /// <summary>
        /// Replace one or more lifetimes in the list with new ones. The old lifetimes are ended.
        /// </summary>
        /// <param name="index">The index at which to start replacing lifetimes.</param>
        /// <param name="count">The number of lifetimes to replace.</param>
        /// <returns>A collection containing the new lifetimes.</returns>
        public IEnumerable<Lifetime> ReplaceRange(int index, int count)
        {
            for (int i = index; i < index + count; ++i)
            {
                _sources[i].EndLifetime();
                _sources[i] = new LifetimeSource();
            }
            return _sources.Skip(index).Take(count).Select(x => x.Lifetime);
        }
    }
}

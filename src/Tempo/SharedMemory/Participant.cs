using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TwistedOak.Util;

namespace Tempo.SharedMemory
{
    /// <summary>
    /// A participant in a transaction. Participants have an arbitrary total order, and must be locked in order of
    /// increasing id to prevent deadlock.
    /// </summary>
    public class Participant
    {
        private static long lastId = 20;

        internal readonly long id;
        internal Transaction owner = null;

        private ConcurrentDictionary<Action, object> unlockedListeners = new ConcurrentDictionary<Action, object>();

        /// <summary>
        /// Construct a new participant, with a unique id.
        /// </summary>
        public Participant()
        {
            this.id = Interlocked.Increment(ref lastId);
        }

        /// <summary>
        /// Register a callback to be invoked when this shared resource is unlocked. The callback
        /// may be invoked from any thread. Each callback may only be concurrently registered once.
        /// </summary>
        /// <param name="lifetime>The lifetime of the subscription</param>
        /// <param name="handler">The handler to invoke. This may be invoked from any thread.</param>
        public void WhenUnlocked(Lifetime lifetime, Action handler)
        {
            if(unlockedListeners.ContainsKey(handler))
            {
                throw new InvalidOperationException("Callback has already been registered");
            }

            unlockedListeners.TryAdd(handler, null);

            lifetime.WhenDead(() =>
                {
                    object value;
                    unlockedListeners.TryRemove(handler, out value);
                });
        }

        internal void OnUnlocked()
        {
            foreach(var handler in unlockedListeners.Keys)
            {
                handler();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tempo.SharedMemory
{
    /// <summary>
    /// Represents a transaction. A transaction contains one or more participants, any number of which may be locked
    /// at any one time. Participants must be locked in increasing order of id to prevent deadlock.
    /// In other words, for any locked participant p in the transaction, there does not exist an unlocked participant u where u.id < p.id
    /// </summary>
    public class Transaction
    {
        private Participant[] sortedParticipants;
        private int waitingFor = 0;

        /// <summary>
        /// Construct a new transaction with the given participants.
        /// </summary>
        /// <param name="desiredParticipants">The participants of the transaction.</param>
        public Transaction(Participant[] desiredParticipants)
        {
            sortedParticipants = new Participant[desiredParticipants.Length];
            Array.Copy(desiredParticipants, sortedParticipants, desiredParticipants.Length);
            Array.Sort(sortedParticipants, (x, y) => x.id.CompareTo(y.id));
        }

        /// <summary>
        /// Attempt to lock all of the participants which have not yet been locked. Returns true if this succeeds.
        /// If it does not succeed, some participants may remain locked.
        /// </summary>
        /// <returns>True if all participants have been locked, false otherwise.</returns>
        public bool TryAcquireAll()
        {
            while (true)
            {
                if(waitingFor >= sortedParticipants.Length)
                {
                    return true;
                }

                if (Acquire(sortedParticipants[waitingFor]))
                {
                    ++waitingFor;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Unlock all locked participants.
        /// </summary>
        public void ReleaseAll()
        {
            for(int i = 0; i < waitingFor; ++i)
            {
                Release(sortedParticipants[i]);
            }

            waitingFor = 0;
        }

        private bool Acquire(Participant p)
        {
            lock (p)
            {
                if (p.owner == null)
                {
                    p.owner = this;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        private void Release(Participant p)
        {
            lock(p)
            {
                p.owner = null;
                p.OnUnlocked();
            }
        }
    }
}

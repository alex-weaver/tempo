using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo.Scheduling;
using TwistedOak.Util;

namespace Tempo.SharedMemory
{
    public interface IHasParticipant
    {
        Participant TransactionParticipant { get; }
    }

    /// <summary>
    /// Provides an interface for shared cells. Using an interface rather than just the Shared<T> class allows the type parameter to be covariant.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IShared<out T> : IHasParticipant
    {
        TaskQueue OwningScope { get; }
        T Get();
    }

    /// <summary>
    /// Represents container shared among threads.
    /// </summary>
    /// <typeparam name="T">The type of value in the container.</typeparam>
    public class Shared<T> : IShared<T>
    {
        private T value;

        public Participant TransactionParticipant { get; private set; }
        public TaskQueue OwningScope { get; private set; }

        /// <summary>
        /// Construct a new container.
        /// </summary>
        /// <param name="initialValue">The value to place in the container.</param>
        public Shared(T initialValue)
        {
            this.TransactionParticipant = new Participant();
            var callingScope = CurrentThread.CurrentContinuousScope();

            this.value = initialValue;
            this.OwningScope = callingScope.taskQueue;
        }

        /// <summary>
        /// Get the value in the container. This method should only ever be called when this participant is locked
        /// by a transaction.
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            return value;
        }
    }
}

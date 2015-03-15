using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;
using Tempo.Scheduling;
using Tempo.Util;
using Tempo.SharedMemory;


namespace Tempo
{
    /// <summary>
    /// Represents a temporal scope which provides a lifetime for continuous expressions. Each TemporalScope object
    /// contains a Lifetime unique to the scope, and a TaskQueue for the thread the scope is to execute on. The Lifetime
    /// object allows continuous expressions to be notified when the scope has ended.
    /// </summary>
    public struct TemporalScope
    {
        /// <summary>The lifetime of the scope. The scope is considered to be ended when the lifetime is ended.
        /// Expressions created in the scope should not schedule any further execution after the lifetime has ended.</summary>
        public readonly Lifetime lifetime;
        /// <summary>The queue that the scoped operators use to schedule execution.</summary>
        public readonly TaskQueue taskQueue;


        /// <summary>Constructs a new temporal scope.</summary>
        public TemporalScope(Lifetime lifetime, TaskQueue taskQueue)
        {
            this.lifetime = lifetime;
            this.taskQueue = taskQueue;
        }


        /// <summary>
        /// Schedules a sequential block for execution. If this method is invoked from a continuous block, the handler will be executed as soon as the
        /// continuous block has been initialized.
        /// When integrating with external events, this method should be used to schedule actions to make sure that the handler is run within
        /// a valid context.
        /// </summary>
        /// <param name="handler">The sequential code to execute.</param>
        public void ScheduleSequentialBlock(Action handler)
        {
            taskQueue.Schedule(
                () => CurrentThread.RunSequentialBlock(handler));
        }
    }
}

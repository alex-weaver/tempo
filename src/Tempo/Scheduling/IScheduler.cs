using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo.Scheduling
{
    /// <summary>
    /// Represents an object that schedules units of work. Classes implementing this interface must run
    /// requested actions sequentially, even if called reentrantly.
    /// Implementation of the Run method should be thread safe.
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Schedules an action to be executed. This method should be thread safe.
        /// </summary>
        /// <param name="handler">The action to be executed.</param>
        void Run(Action handler);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    public static class Timing
    {
        /// <summary>
        /// Schedule a sequential block to execute after a specific interval has elapsed. Note that the interval is a lower bound on
        /// when the handler will be exectued, not an exact guarantee. If the scope ends before the interval has elapsed, the handler
        /// will not be executed.
        /// </summary>
        /// <param name="interval">The interval to wait before scheduling the handler.</param>
        /// <param name="handler">The sequential block to execute.</param>
        public static void After(TimeSpan interval, Action handler)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            DateTime targetTimeUtc = DateTime.UtcNow + interval;
            callingScope.taskQueue.ScheduleAt(callingScope.lifetime, targetTimeUtc, () => CurrentThread.RunSequentialBlock(handler));
        }

        /// <summary>
        /// Repeatedly schedule a sequential block to be executed at a specific interval, until the scope ends.
        /// </summary>
        /// <param name="interval">The interval between each invocation of the handler.</param>
        /// <param name="runImmediately">If true, the first run of the handler occurs immediately. If false, the first run will occur after interval has elapsed.</param>
        /// <param name="handler">The sequential block to execute.</param>
        public static void Every(TimeSpan interval, bool runImmediately, Action handler)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            DateTime nextInvocationUtc = DateTime.UtcNow + (runImmediately ? TimeSpan.Zero : interval);

            Action innerHandler = null;

            innerHandler = new Action(() =>
            {
                CurrentThread.RunSequentialBlock(() => handler());
                nextInvocationUtc += interval;
                callingScope.taskQueue.ScheduleAt(callingScope.lifetime, nextInvocationUtc, innerHandler);
            });

            callingScope.taskQueue.ScheduleAt(callingScope.lifetime, nextInvocationUtc, innerHandler);
        }
    }
}

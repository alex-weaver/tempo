using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo.Scheduling
{
    /// <summary>
    /// An implementation of IScheduler for threads which do not need to perform any work except via the IScheduler.
    /// After constructing a ThreadScheduler, a client should call RunLoop, which will block the thread and wait for work.
    /// </summary>
    public class ThreadScheduler : IScheduler
    {
        private ConcurrentQueue<Action> taskQueue = new ConcurrentQueue<Action>();

        /// <summary>
        /// Construct a new scheduler.
        /// </summary>
        public ThreadScheduler()
        {
        }

        /// <summary>
        /// Schedule an action to be run. This method is thread safe.
        /// </summary>
        /// <param name="handler">The action to run.</param>
        public void Run(Action handler)
        {
            taskQueue.Enqueue(handler);
        }

        private Action DequeueTask()
        {
            Action result;
            if(taskQueue.TryDequeue(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Blocks the current thread and waits for work to schedule. When the given lifetime is cancelled,
        /// this method will return once its current work item is completed.
        /// </summary>
        /// <param name="lifetime">The lifetime of the run loop.</param>
        public void RunLoop(TwistedOak.Util.Lifetime lifetime)
        {
            while (!lifetime.IsDead)
            {
                var nextTask = DequeueTask();

                if (nextTask == null)
                {
                    System.Threading.Thread.Sleep(0);
                }
                else
                {
                    nextTask();
                }
            }
        }
    }
}

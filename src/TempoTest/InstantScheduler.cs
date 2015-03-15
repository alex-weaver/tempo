using Tempo.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TempoTest
{
    public class InstantScheduler : IScheduler
    {
        private int threadId;

        private Queue<Action> pending = new Queue<Action>();
        private bool isRunning = false;


        public InstantScheduler()
        {
            this.threadId = Thread.CurrentThread.ManagedThreadId;
        }

        public void Run(Action handler)
        {
            if(Thread.CurrentThread.ManagedThreadId != threadId)
            {
                throw new NotImplementedException("InstantScheduler does not support calls from other threads");
            }

            pending.Enqueue(handler);
            RunIfNeeded();
        }

        private void RunIfNeeded()
        {
            if(!isRunning)
            {
                isRunning = true;
                while(pending.Any())
                {
                    var handler = pending.Dequeue();
                    handler();
                }
                isRunning = false;
            }
        }
    }
}

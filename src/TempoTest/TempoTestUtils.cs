using Tempo;
using Tempo.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwistedOak.Util;

namespace TempoTest
{
    public static class TempoTestUtils
    {
        public static void Run(Action handler)
        {
            // If another test has been run in this thread and failed to shut down properly,
            // we need to reset the thread so that ThreadScope.Init will work
            CurrentThread.ResetThread();

            var scheduler = new InstantScheduler();
            var taskQueue = new TaskQueue(scheduler);
            var lifetimeSrc = new LifetimeSource();

            // handler is not run in an isolated (scheduled) block, so it can be used for testing isolation
            CurrentThread.Init(scheduler, lifetimeSrc.Lifetime, handler);
            lifetimeSrc.EndLifetime();
        }

        public static void RunDelayed(Action<Action> handler)
        {
            // If another test has been run in this thread and failed to shut down properly,
            // we need to reset the thread so that ThreadScope.Init will work
            CurrentThread.ResetThread();

            var scheduler = new ThreadScheduler();
            var taskQueue = new TaskQueue(scheduler);
            var lifetimeSrc = new LifetimeSource();

            CurrentThread.Init(scheduler, lifetimeSrc.Lifetime, () =>
                {
                    handler(() => lifetimeSrc.EndLifetime());
                });

            scheduler.RunLoop(lifetimeSrc.Lifetime);
        }
    }
}

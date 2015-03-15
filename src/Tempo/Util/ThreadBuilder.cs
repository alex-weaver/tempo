using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tempo.Scheduling;
using TwistedOak.Util;

namespace Tempo.Util
{
    /// <summary>
    /// Helper class for constructing a background thread.
    /// </summary>
    public static class ThreadBuilder
    {
        /// <summary>
        /// Creates a new background thread. When the thread is started, it constructs a scheduler and top-level scope, then
        /// hands control to the scheduler to begin processing work.
        /// </summary>
        /// <param name="threadLifetime">The lifetime of the thread. When the lifetime ends, the thread will end after completing its current work item.</param>
        /// <param name="threadContextConstructor">A constructor to build the new thread's top-level scope.</param>
        public static void Build(Lifetime threadLifetime, Action threadContextConstructor)
        {
            var thread = new Thread(new ThreadStart(() =>
            {
                var scheduler = new ThreadScheduler();
                var topLevelScopeLifetimeSrc = new LifetimeSource();

                var taskQueue = CurrentThread.Init(scheduler, topLevelScopeLifetimeSrc.Lifetime, threadContextConstructor);

                threadLifetime.WhenDead(() =>
                    {
                        scheduler.Run(() => taskQueue.Schedule(() => topLevelScopeLifetimeSrc.EndLifetime()));
                    });

                scheduler.RunLoop(threadLifetime);
            }));

            thread.Start();
        }
    }
}

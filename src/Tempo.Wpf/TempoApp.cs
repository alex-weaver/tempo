using Tempo.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using TwistedOak.Util;

namespace Tempo.Wpf
{
    /// <summary>
    /// Provides basic functionality for WPF applications. Abstracts the construction and tear-down of a scheduler, task queue and top-level scope.
    /// </summary>
    public static class TempoApp
    {
        /// <summary>
        /// Register the given application so that a top-level reactive scope will be constructed when the application starts, and destroyed when the
        /// application exits. If the destroyScopeOnExit argument is false, the scope will not be destroyed when the application exits. Do
        /// this if there is no essential tear-down work the application needs to do, in which case it will be left to the OS to free any
        /// resources the application is using.
        /// </summary>
        /// <param name="application">The WPF application instance.</param>
        /// <param name="destroyScopeOnExit">True if the top level reactive scope should be destroyed when the application exits; false otherwise.</param>
        /// <param name="whileRunning">The constructor for the application's top-level reactive scope</param>
        public static void Init(Application application, bool destroyScopeOnExit, Action whileRunning)
        {
            LifetimeSource appLifetimeSrc = null;
            
            application.Startup += (s, e) =>
                {
                    var scheduler = new WpfScheduler(application.Dispatcher);
                    appLifetimeSrc = new LifetimeSource();

                    CurrentThread.Init(scheduler, appLifetimeSrc.Lifetime, whileRunning);
                };

            application.Exit += (s, e) =>
                {
                    if (destroyScopeOnExit)
                    {
                        appLifetimeSrc.EndLifetime();
                    }

                    appLifetimeSrc = null;
                };
        }
    }
}

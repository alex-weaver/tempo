using Tempo.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using TwistedOak.Util;

namespace Tempo.Wpf
{
    /// <summary>
    /// Provides an implementation of IScheduler which uses a WPF dispatcher to perform work. This allows
    /// Tempo and WPF to cooperate on the same thread.
    /// </summary>
    public class WpfScheduler : IScheduler
    {
        private Dispatcher _dispatcher;


        public WpfScheduler(Dispatcher dispatcher)
        {
            this._dispatcher = dispatcher;
        }

        public void Run(Action handler)
        {
            _dispatcher.BeginInvoke(handler, DispatcherPriority.Background);
        }
    }
}

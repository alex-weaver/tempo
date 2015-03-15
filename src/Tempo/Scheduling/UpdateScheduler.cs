using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo.Scheduling
{
    /// <summary>
    /// A helper class for idempotently scheduling updates via an IScheduler. There will never be more than
    /// one update scheduled at the same time.
    /// </summary>
    public class UpdateScheduler
    {
        private bool updateScheduled = false;
        private IScheduler scheduler;
        private Action update;


        /// <summary>
        /// Construct an update scheduler, given an IScheduler to allocate work and a callback to invoke to perform an update.
        /// </summary>
        /// <param name="scheduler">The scheduler to allocate work.</param>
        /// <param name="update">The callback to invoke to perform an update.</param>
        public UpdateScheduler(IScheduler scheduler, Action update)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            if (update == null) throw new ArgumentNullException("update");

            this.scheduler = scheduler;
            this.update = update;
        }

        /// <summary>
        /// Notify the udpate scheduler that an update should be scheduled if one is not already pending.
        /// </summary>
        public void NeedsUpdate()
        {
            if(!updateScheduled)
            {
                updateScheduled = true;
                scheduler.Run(HandleUpdate);
            }
        }

        private void HandleUpdate()
        {
            updateScheduled = false;
            update();
        }
    }
}

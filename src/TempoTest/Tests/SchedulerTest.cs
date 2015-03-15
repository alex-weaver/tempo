using Tempo;
using Tempo.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempoTest.Tests
{
    [TestClass]
    public class SchedulerTest
    {
        [TestMethod]
        public void NestedAtomicTest()
        {
            var scheduler = new InstantScheduler();
            var taskQueue = new TaskQueue(scheduler);

            int value = 1;
            taskQueue.Schedule(() =>
            {
                taskQueue.Schedule(() =>
                {
                    value = 2;
                });

                value = 3;
                Assert.AreEqual(3, value);
            });

            Assert.AreEqual(2, value);
        }

        [TestMethod]
        public void UpdateableIsolation()
        {
            var scheduler = new InstantScheduler();
            var taskQueue = new TaskQueue(scheduler);
            var lifetimeSrc = new TwistedOak.Util.LifetimeSource();

            int updateCount = 0;

            var updateable = taskQueue.CreateUpdateable(lifetimeSrc.Lifetime, () =>
                {
                    ++updateCount;
                });

            
            Assert.AreEqual(1, updateCount);

            updateCount = 0;
            taskQueue.Schedule(() =>
            {
                updateable.NeedsUpdate();
                updateable.NeedsUpdate();
            });

            Assert.AreEqual(1, updateCount);

            lifetimeSrc.EndLifetime();
        }
    }
}

using Tempo;
using Tempo.Scheduling;
using Tempo.SharedMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TwistedOak.Util;
using Tempo.Util;

namespace TempoTest.Tests
{
    [TestClass]
    public class ThreadingTest
    {
        [TestMethod]
        public void ThreadScheduler()
        {
            var scheduler = new InstantScheduler();
            var taskQueue = new TaskQueue(scheduler);
            var threadLifetimeSource = new LifetimeSource();

            int mainThreadId = Thread.CurrentThread.ManagedThreadId;
            int innerThreadId = -1;

            ThreadBuilder.Build(threadLifetimeSource.Lifetime, () =>
                {
                    innerThreadId = Thread.CurrentThread.ManagedThreadId;

                    Events.WhenEnded(() =>
                    {
                        var innerDestructorThreadId = Thread.CurrentThread.ManagedThreadId;
                        Assert.AreEqual(innerThreadId, innerDestructorThreadId, "Destructor thread id");
                    });
                });

            while(innerThreadId == -1)
            {
                Thread.Sleep(0);
            }

            Assert.IsTrue(mainThreadId != innerThreadId, "Thread ids must not be equal");

            threadLifetimeSource.EndLifetime();
        }


        // If this test fails, it doesn't terminate.
        [TestMethod]
        public void CrossThreadList()
        {
            TempoTestUtils.RunDelayed(endTest =>
            {
                var instanceCount = new InstanceCount();

                var listCell = new ListCell<TestRefCounted>();
                var sharedCell = new Shared<ListCell<TestRefCounted>>(listCell);

                var initComplete = new Shared<MemoryCell<bool>>(new MemoryCell<bool>(false));

                for (int i = 0; i < 10; ++i)
                {
                    var value = new TestRefCounted(instanceCount);
                    listCell.Add(value);
                    value.Release();
                }
                Assert.AreEqual(10, instanceCount.value);

                Threading.RunThread(() =>
                    {
                        var localList = Threading.WatchOnCurrentThread(sharedCell);

                        Events.AnyChange(localList, () =>
                            {
                                if(localList.Count == 11)
                                {
                                    Threading.Lock(initComplete, cell =>
                                    {
                                        cell.Cur = true;
                                    });
                                }
                            });
                    });

                Events.Once(() =>
                    {
                        Threading.Lock(sharedCell, list =>
                            {
                                list.Add(new TestRefCounted(instanceCount));
                            });
                    });

                Threading.WatchOnCurrentThread(initComplete)
                .WhenTrue(() =>
                    {
                        endTest();
                    });

            });
        }
    }
}

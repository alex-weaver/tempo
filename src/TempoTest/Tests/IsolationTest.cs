using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tempo;

namespace TempoTest.Tests
{
    [TestClass]
    public class IsolationTest
    {
        [TestMethod]
        public void TwoValueCellsIsolation()
        {
            TempoTestUtils.Run(() =>
                {
                    var cell1 = new MemoryCell<int>(5);
                    var cell2 = new MemoryCell<int>(6);

                    int changeCount = 0;
                    Events.AnyChange(new ICell[] { cell1, cell2 }, () =>
                        {
                            ++changeCount;
                        });

                    Assert.AreEqual(1, changeCount);

                    Events.Once(() =>
                        {
                            cell1.Set(10);
                            cell2.Set(10);
                        });

                    Assert.AreEqual(2, changeCount);

                });
        }

        [TestMethod]
        public void NestedDoBlockTest()
        {
            TempoTestUtils.Run(() =>
            {
                int value = 1;
                Events.Once(() =>
                {
                    Events.Once(() =>
                    {
                        value = 2;
                    });

                    value = 3;
                    Assert.AreEqual(3, value);
                });

                Assert.AreEqual(2, value);
            });
        }
        
        [TestMethod]
        public void WhenHandlerIsolation()
        {
            TempoTestUtils.Run(() =>
                {
                    var cell1 = new MemoryCell<int>(5);
                    var cell2 = new MemoryCell<int>(6);

                    int changeCount = 0;
                    Events.AnyChange(new ICell[] { cell1, cell2 }, () =>
                    {
                        ++changeCount;
                    });

                    Assert.AreEqual(1, changeCount);

                    cell1.When(x => x == 5, () =>
                    {
                        cell1.Set(10);
                        cell2.Set(10);
                    });

                    Assert.AreEqual(2, changeCount);

                });
        }

        [TestMethod]
        public void LatestIsolation()
        {
            TempoTestUtils.Run(() =>
                {
                    var cell = new MemoryCell<int>(80);

                    int changeCount = 0;
                    int lastValue = cell.Cur;

                    cell.Latest(value =>
                        {
                            lastValue = value;
                            changeCount++;
                        });

                    Assert.AreEqual(80, lastValue);
                    Assert.AreEqual(1, changeCount);

                    cell.Cur = 81;

                    Assert.AreEqual(81, lastValue);
                    Assert.AreEqual(2, changeCount);

                    Events.Once(() =>
                        {
                            cell.Cur = 82;
                            cell.Cur = 83;
                            cell.Cur = 84;
                        });

                    Assert.AreEqual(84, lastValue);
                    Assert.AreEqual(3, changeCount);
                });
        }
    }
}

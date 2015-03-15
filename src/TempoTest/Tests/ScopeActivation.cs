using Tempo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TempoTest.Tests
{
    [TestClass]
    public class ScopeActivation
    {
        [TestMethod]
        public void WhilstActivation()
        {
            TempoTestUtils.Run(() =>
                {
                    var activation = new MemoryCell<bool>(false);

                    bool hasActivated = false;
                    activation.WhilstTrue(() =>
                        {
                            hasActivated = true;
                        });

                    activation.Cur = true;
                    Assert.IsTrue(hasActivated);
                });
        }

        [TestMethod]
        public void WithLatestActivation()
        {
            int activeInnerScopes = 0;
            TempoTestUtils.Run(() =>
                {
                    var cell = new MemoryCell<int>(5);

                    int activationCount = 0;
                    var transformed = cell.WithLatest(value =>
                        {
                            ++activationCount;

                            ++activeInnerScopes;
                            Events.WhenEnded(() => activeInnerScopes--);

                            return CellBuilder.Const(value);
                        });

                    Assert.AreEqual(1, activationCount);
                    Assert.AreEqual(5, transformed.Cur);
                    Assert.AreEqual(1, activeInnerScopes);

                    cell.Cur = 5920;
                    Assert.AreEqual(2, activationCount);
                    Assert.AreEqual(5920, transformed.Cur);
                    Assert.AreEqual(1, activeInnerScopes);
                });

            Assert.AreEqual(0, activeInnerScopes);
        }

        [TestMethod]
        public void WithEachActivation()
        {
            int concurrentActivations = 0;
            TempoTestUtils.Run(() =>
                {
                    var cell = new ListCell<long>();
                    cell.Add(314);
                    cell.Add(2600);

                    var transformed = cell.WithEach(value =>
                        {
                            concurrentActivations++;
                            Events.WhenEnded(() => concurrentActivations--);
                            return CellBuilder.Const(value * 10);
                        });

                    Assert.AreEqual(2, concurrentActivations);
                    Assert.AreEqual(3140, transformed[0]);
                    Assert.AreEqual(26000, transformed[1]);

                    cell.Add(-1);
                    Assert.AreEqual(3, concurrentActivations);
                    Assert.AreEqual(-10, transformed[2]);
                });

            Assert.AreEqual(0, concurrentActivations);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo;
using TwistedOak.Util;

namespace TempoTest.Tests
{
    [TestClass]
    public class ResourceBrokerTests
    {
        [TestMethod]
        public void RequestBrokerTest()
        {
            TempoTestUtils.Run(() =>
                {
                    var broker = new RequestBroker<int, string>(null, requests => requests.Max().ToString());

                    var activation1 = new MemoryCell<bool>(true);
                    var request1 = new MemoryCell<int>(20);

                    activation1.WhilstTrue(() =>
                        {
                            broker.ApplyRequest(request1);
                        });

                    var activation2 = new MemoryCell<bool>(true);
                    var request2 = new MemoryCell<int>(100);

                    activation2.WhilstTrue(() =>
                    {
                        broker.ApplyRequest(request2);
                    });

                    Assert.AreEqual("100", broker.Aggregate.Cur);
                    Assert.AreEqual("100", broker.Aggregate.Cur);

                    request1.Cur = 101;
                    Assert.AreEqual("101", broker.Aggregate.Cur);
                    Assert.AreEqual("101", broker.Aggregate.Cur);

                    request2.Cur = 99;
                    activation1.Cur = false;
                    Assert.AreEqual("99", broker.Aggregate.Cur);
                });
        }
    }
}

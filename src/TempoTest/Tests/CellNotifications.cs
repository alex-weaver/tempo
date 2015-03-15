using Tempo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempoTest.Tests
{
    [TestClass]
    public class CellNotifications
    {
        private static TwistedOak.Util.LifetimeSource immortal = new TwistedOak.Util.LifetimeSource();

        [TestMethod]
        public void ValueCellNoDestroyNotify()
        {
            TempoTestUtils.Run(() =>
                {
                    var cell = new MemoryCell<double>(12.0);

                    cell.ListenForChanges(immortal.Lifetime, () =>
                        {
                            Assert.Fail();
                        });
                });
        }

        [TestMethod]
        public void ListCellNoDestroyNotify()
        {
            TempoTestUtils.Run(() =>
            {
                var cell = new ListCell<int>();

                cell.ListenForChanges(immortal.Lifetime, () =>
                {
                    Assert.Fail();
                });
            });
        }

        [TestMethod]
        public void ValueCellUpdates()
        {
            TempoTestUtils.Run(() =>
            {
                var cell = new MemoryCell<double>(12.0);
                int notifyCount = 0;
                cell.ListenForChanges(CurrentThread.CurrentContinuousScope().lifetime, () =>
                {
                    ++notifyCount;
                });

                cell.Set(cell.Cur);
                Assert.AreEqual(0, notifyCount);
                cell.Set(95);
                Assert.AreEqual(1, notifyCount);
                cell.Set(-0.02);
                Assert.AreEqual(2, notifyCount);
            });
        }

        [TestMethod]
        public void ListCellRemoveNotify()
        {
            TempoTestUtils.Run(() =>
            {
                var cell = new ListCell<int>();

                for (int i = 0; i < 15; ++i)
                {
                    cell.Add(i);
                }

                cell.ListenChanges(immortal.Lifetime, changes =>
                {
                    Assert.AreEqual(3, changes.OldItemCount);

                    Assert.IsNull(changes.NewItems);
                    
                });

                cell.RemoveRange(0, 3);
            });
        }
    }
}

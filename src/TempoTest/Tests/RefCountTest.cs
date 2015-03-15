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
    public class RefCountTest
    {
        [TestMethod]
        public void ValueCellRefCount()
        {
            TempoTestUtils.Run(() =>
                {
                    var instanceCount = new InstanceCount();

                    var initialValue = new TestRefCounted(instanceCount);
                    var cell = new MemoryCell<TestRefCounted>(initialValue);
                    initialValue.Release();

                    Assert.AreEqual(1, instanceCount.value);

                    var newValue = new TestRefCounted(instanceCount);
                    cell.Cur = newValue;
                    newValue.Release();

                    Assert.AreEqual(1, instanceCount.value);

                    cell.Cur = null;

                    Assert.AreEqual(0, instanceCount.value);
                });
        }

        [TestMethod]
        public void RefCountHelpersReleaseRange()
        {
            var instanceCount = new InstanceCount();
            var items = new List<TestRefCounted>();

            for(int i = 0; i < 8; ++i)
            {
                var value = new TestRefCounted(instanceCount);
                items.Add(value);
            }
            Assert.AreEqual(8, instanceCount.value);

            RefCountHelpers.ReleaseRange(items);
            Assert.AreEqual(0, instanceCount.value);
        }

        [TestMethod]
        public void ListCellSelectRefCount()
        {
            TempoTestUtils.Run(() =>
            {
                var instanceCount = new InstanceCount();

                var listCell = new ListCell<TestRefCounted>();

                for (int i = 0; i < 10; ++i)
                {
                    var value = new TestRefCounted(instanceCount);
                    listCell.Add(value);
                    value.Release();
                }
                Assert.AreEqual(10, instanceCount.value);

                listCell.RemoveAt(0);
                Assert.AreEqual(9, instanceCount.value);

                listCell.RemoveRange(3, 4);
                Assert.AreEqual(5, instanceCount.value);

                listCell.Clear();
                Assert.AreEqual(0, instanceCount.value);
            });
        }

        [TestMethod]
        public void TemporalScopeRefCount()
        {
            TempoTestUtils.Run(() =>
            {
                var instanceCount = new InstanceCount();

                var innerScopeActive = new MemoryCell<bool>(false);

                innerScopeActive.WhilstTrue(() =>
                    {
                        var valueCell = new MemoryCell<TestRefCounted>(null);
                        var listCell = new ListCell<TestRefCounted>();

                        var newValue = new TestRefCounted(instanceCount);
                        valueCell.Cur = newValue;
                        newValue.Release();

                        for (int i = 0; i < 10; ++i)
                        {
                            var value = new TestRefCounted(instanceCount);
                            listCell.Add(value);
                            value.Release();
                        }
                    });

                Assert.AreEqual(0, instanceCount.value);

                innerScopeActive.Cur = true;
                Assert.AreEqual(11, instanceCount.value);

                innerScopeActive.Cur = false;
                Assert.AreEqual(0, instanceCount.value);
            });
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo;

namespace TempoTest.Tests
{
    [TestClass]
    public class MemoryCellTest
    {
        [TestMethod]
        public void MemoryCellFlatten()
        {
            TempoTestUtils.Run(() =>
                {
                    var outer = new MemoryCell<MemoryCell<int>>(null);
                    var inner = new MemoryCell<int>(10);
                    outer.Cur = inner;

                    var flattened = outer.Flatten();
                    int flattenedChanges = 0;
                    flattened.ListenForChanges(CurrentThread.CurrentContinuousScope().lifetime, () => flattenedChanges++);

                    Assert.AreEqual(10, flattened.Cur);
                    Assert.AreEqual(0, flattenedChanges);
                    
                    inner.Cur = 11;
                    Assert.AreEqual(11, flattened.Cur);
                    Assert.AreEqual(1, flattenedChanges);

                    var inner2 = new MemoryCell<int>(1000);
                    outer.Cur = inner2;
                    Assert.AreEqual(1000, flattened.Cur);
                    Assert.AreEqual(2, flattenedChanges);
                });
        }

        [TestMethod]
        public void MemoryCellMerge()
        {
            TempoTestUtils.Run(() =>
                {
                    var cell1 = new MemoryCell<int>(5);
                    var cell2 = new MemoryCell<string>("orange");
                    var cell3 = new MemoryCell<double>(0.1);

                    var merged = CellBuilder.Merge(cell1, cell2, cell3, (val1, val2, val3) =>
                        {
                            return Tuple.Create(val1, val2, val3);
                        });

                    int mergedChangeCount = 0;
                    merged.ListenForChanges(CurrentThread.CurrentContinuousScope().lifetime, () =>
                        {
                            mergedChangeCount++;
                        });

                    Assert.AreEqual(Tuple.Create(5, "orange", 0.1), merged.Cur);
                    Assert.AreEqual(0, mergedChangeCount);

                    cell1.Cur = 9;
                    Assert.AreEqual(Tuple.Create(9, "orange", 0.1), merged.Cur);
                    Assert.AreEqual(1, mergedChangeCount);

                    cell2.Cur = "green";
                    Assert.AreEqual(Tuple.Create(9, "green", 0.1), merged.Cur);
                    Assert.AreEqual(2, mergedChangeCount);

                    cell3.Cur = 1984.0;
                    Assert.AreEqual(Tuple.Create(9, "green", 1984.0), merged.Cur);
                    Assert.AreEqual(3, mergedChangeCount);
                });
        }
    }
}

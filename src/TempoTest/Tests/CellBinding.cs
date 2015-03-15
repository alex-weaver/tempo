using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo;

namespace TempoTest.Tests
{
    [TestClass]
    public class CellBinding
    {
        [TestMethod]
        public void MemoryCellBinding()
        {
            TempoTestUtils.Run(() =>
                {
                    var source = new MemoryCell<int>(9010);
                    var destination = new MemoryCell<int>(2000);

                    source.Bind(destination);

                    Assert.AreEqual(9010, destination.Cur);

                    source.Cur = 1010;
                    Assert.AreEqual(1010, destination.Cur);
                });
        }


        [TestMethod]
        public void ListCellBinding()
        {
            TempoTestUtils.Run(() =>
                {
                    var source = new ListCell<string>();
                    for(int i = 0; i < 100000; ++i)
                    {
                        source.Add(Guid.NewGuid().ToString());
                    }

                    var destinationIList = new List<string>();
                    var destinationCell = new ListCell<string>();

                    source.Bind(destinationIList);
                    source.Bind(destinationCell);

                    Assert.IsTrue(source.Cur.SequenceEqual(destinationIList));
                    Assert.IsTrue(source.Cur.SequenceEqual(destinationCell.Cur));

                    source.RemoveRange(100, source.Count - 100);
                    Assert.IsTrue(source.Cur.SequenceEqual(destinationIList));
                    Assert.IsTrue(source.Cur.SequenceEqual(destinationCell.Cur));

                    source.ReplaceRange(50, Enumerable.Range(0, 50).Select(x => Guid.NewGuid().ToString()).ToList());
                    Assert.IsTrue(source.Cur.SequenceEqual(destinationIList));
                    Assert.IsTrue(source.Cur.SequenceEqual(destinationCell.Cur));
                });
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo;

namespace TempoTest.Tests
{
    [TestClass]
    public class ListExtensionsTest
    {
        [TestMethod]
        public void ListSelect()
        {
            TempoTestUtils.Run(() =>
                {
                    var source = new ListCell<int>();
                    for (int i = 0; i < 100; ++i)
                    {
                        source.Add(i);
                    }

                    var transformed = source.Select(x => x.ToString());

                    CollectionAssert.AreEqual(
                        Enumerable.Range(0, 100).Select(x => x.ToString()).ToList(),
                        transformed.Cur.ToList());

                    source.RemoveRange(0, 50);

                    CollectionAssert.AreEqual(
                        Enumerable.Range(50, 50).Select(x => x.ToString()).ToList(),
                        transformed.Cur.ToList());

                });
        }

        [TestMethod]
        public void ListElementAt()
        {
            TempoTestUtils.Run(() =>
                {
                    var list = new ListCell<string>();
                    for (int i = 0; i < 100; ++i)
                    {
                        list.Add(i.ToString());
                    }
                    var index = new MemoryCell<int>(20);

                    var element = list.ElementAt(index);
                    Assert.AreEqual("20", element.Cur);

                    index.Cur = 90;
                    Assert.AreEqual("90", element.Cur);

                    list.RemoveRange(0, 5);
                    Assert.AreEqual("95", element.Cur);
                });
        }

        [TestMethod]
        public void ListFlatten()
        {
            TempoTestUtils.Run(() =>
                {
                    var list = new ListCell<MemoryCell<int>>();
                    for (int i = 0; i < 100; ++i)
                    {
                        list.Add(new MemoryCell<int>(i + 1000));
                    }

                    var flattened = list.Flatten();

                    CollectionAssert.AreEqual(
                        Enumerable.Range(1000, 100).ToList(),
                        flattened.Cur.ToList());

                    foreach(var item in list.Cur)
                    {
                        item.Cur += 1000;
                    }

                    CollectionAssert.AreEqual(
                        Enumerable.Range(2000, 100).ToList(),
                        flattened.Cur.ToList());

                    for (int i = 0; i < 100; ++i)
                    {
                        list[i] = new MemoryCell<int>(i + 3000);
                    }

                    CollectionAssert.AreEqual(
                        Enumerable.Range(3000, 100).ToList(),
                        flattened.Cur.ToList());
                });
        }
    }
}

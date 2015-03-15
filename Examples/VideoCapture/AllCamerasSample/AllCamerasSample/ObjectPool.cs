using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwistedOak.Util;

namespace VideoTest
{
    public class ObjectPool<TKey, TValue>
    {
        private class RecentItem
        {
            public readonly TKey Key;
            public readonly TValue Value;

            public RecentItem(TKey key, TValue value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        private class DictionaryEntry
        {
            public readonly TValue Value;
            public readonly LinkedListNode<RecentItem> LruEntry;

            public DictionaryEntry(TValue value, LinkedListNode<RecentItem> lruEntry)
            {
                this.Value = value;
                this.LruEntry = lruEntry;
            }
        }


        private Dictionary<TKey, List<DictionaryEntry>> valueGroups = new Dictionary<TKey, List<DictionaryEntry>>();
        private LinkedList<RecentItem> recentlyUsed = new LinkedList<RecentItem>(); // most recently used is at end of the list

        private Func<TKey, TValue> alloc;
        private long maxPoolSize;
        private Func<TValue, long> sizeFunc;
        private long currentSize = 0;

        bool freed = false;
        public ObjectPool(Func<TKey, TValue> allocator, Func<TValue, long> sizeCalculator, long maxSize)
        {
            this.alloc = allocator;
            this.sizeFunc = sizeCalculator;
            this.maxPoolSize = maxSize;

            Tempo.CurrentThread.CurrentContinuousScope().lifetime.WhenDead(() =>
                {
                    freed = true;
                    recentlyUsed.Clear();
                    valueGroups.Clear();
                    alloc = null;
                    sizeFunc = null;
                });
        }

        public ObjectPool(Func<TKey, TValue> allocator, long maxObjectCount)
        {
            this.alloc = allocator;
            this.sizeFunc = val => 1;
            this.maxPoolSize = maxObjectCount;
        }

        public TValue Get(Lifetime lifetime, TKey key)
        {
            if (freed)
                return default(TValue);

            TValue result;

            List<DictionaryEntry> valuesForKey;
            if(valueGroups.TryGetValue(key, out valuesForKey))
            {
                if (valuesForKey.Any())
                {
                    var index = valuesForKey.Count - 1;
                    var entry = valuesForKey[index];
                    valuesForKey.RemoveAt(index);

                    result = entry.Value;
                    recentlyUsed.Remove(entry.LruEntry);
                }
                else
                {
                    result = alloc(key);
                }
            }
            else
            {
                result = alloc(key);
            }

            lifetime.WhenDead(() =>
                {
                    AddToPool(key, result);
                });

            return result;
        }

        private void AddToPool(TKey key, TValue value)
        {
            if (freed)
                return;

            var newValueSize = sizeFunc(value);

            while(recentlyUsed.Any() && (currentSize + newValueSize) > maxPoolSize)
            {
                Remove(recentlyUsed.First);
            }


            var newRecent = new LinkedListNode<RecentItem>(new RecentItem(key, value));
            var newEntry = new DictionaryEntry(value, newRecent);

            recentlyUsed.AddLast(newRecent);

            List<DictionaryEntry> valuesForKey;
            if (valueGroups.TryGetValue(key, out valuesForKey))
            {
                valuesForKey.Add(newEntry);
            }
            else
            {
                valuesForKey = new List<DictionaryEntry>();
                valuesForKey.Add(newEntry);
                valueGroups[key] = valuesForKey;
            }

            currentSize += newValueSize;
        }

        private void Remove(LinkedListNode<RecentItem> recentItem)
        {
            var removedSize = sizeFunc(recentItem.Value.Value);

            recentlyUsed.Remove(recentItem);

            var group = valueGroups[recentItem.Value.Key];
            group.RemoveAll(x => x.Value.Equals(recentItem.Value.Value));

            currentSize -= removedSize;
        }
    }
}

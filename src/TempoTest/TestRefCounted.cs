using Tempo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempoTest
{
    public class InstanceCount
    {
        public int value = 0;

        public InstanceCount()
        {
        }
    }

    public class TestRefCounted : RefCountedSafe
    {
        private InstanceCount _count;

        
        public TestRefCounted(InstanceCount count)
        {
            this._count = count;

            _count.value++;
        }

        protected override void Destroy()
        {
            _count.value--;
        }
    }
}

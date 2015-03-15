using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo.Scheduling
{
    public interface IUpdateableHandle
    {
        void NeedsUpdate();
    }

    public class AnonymousUpdateableHandle : IUpdateableHandle
    {
        private Action needsUpdate;

        public AnonymousUpdateableHandle(Action needsUpdate)
        {
            if (needsUpdate == null) throw new ArgumentNullException("needsUpdate");
            this.needsUpdate = needsUpdate;
        }

        public void NeedsUpdate()
        {
            needsUpdate();
        }
    }
}

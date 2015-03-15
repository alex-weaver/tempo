using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo.Util
{
    public class AnonymousCellWrite<T> : ICellWrite<T>
    {
        private Action<T> onWrite;

        public AnonymousCellWrite(Action<T> onWrite)
        {
            if (onWrite == null) throw new ArgumentNullException("onWrite");
            this.onWrite = onWrite;
        }

        public T Cur
        {
            set { onWrite(value); }
        }

        public void Set(T value)
        {
            onWrite(value);
        }
    }
}

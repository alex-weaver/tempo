using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib.Util
{
    public abstract class NativeHandle<T> : IDisposable
    {
        protected T _ptr;
        private bool _disposed = false;

        public NativeHandle(T ptr)
        {
            this._ptr = ptr;

        }

        ~NativeHandle()
        {
            DoDispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DoDispose();
        }


        public T Ptr { get { return _ptr; } }

        protected abstract void DisposeUnmanaged();

        private void DoDispose()
        {
            if (_disposed)
                return;

            DisposeUnmanaged();

            _disposed = true;
        }
    }
}

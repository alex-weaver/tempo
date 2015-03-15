using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib.Util
{
    public class ComObject<T> : NativeHandle<T> where T : class
    {
        public ComObject(T ptr)
            : base(ptr)
        {
        }

        protected override void DisposeUnmanaged()
        {
            if (_ptr != null)
            {
                Marshal.ReleaseComObject(_ptr);
            }
        }
    }
}

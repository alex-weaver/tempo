using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib.Util
{
    internal static class ComHelpers
    {
        public static T AddRcwRef<T>(T t)
        {
            IntPtr ptr = Marshal.GetIUnknownForObject(t);
            try
            {
                return (T)Marshal.GetObjectForIUnknown(ptr);
            }
            finally
            {
                Marshal.Release(ptr);
            }
        }
    }
}

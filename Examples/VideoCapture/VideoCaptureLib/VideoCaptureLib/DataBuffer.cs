using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib
{
    public class DataBuffer : IDisposable
    {
        public IntPtr Data { get; private set; }
        public int Length { get; private set; }


        public DataBuffer(int length)
        {
            this.Length = length;
            Data = Marshal.AllocHGlobal(length);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Data);
            Data = IntPtr.Zero;
        }
    }
}

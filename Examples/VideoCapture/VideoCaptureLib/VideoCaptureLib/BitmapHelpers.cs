using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib
{
    public static class BitmapHelpers
    {
        public static void Copy(IntPtr sourcePixels, int sourceStride, IntPtr targetPixels, int targetStride, int height)
        {
            IntPtr sourcePtr = sourcePixels;
            IntPtr targetPtr = targetPixels;

            var lineLength = (uint)Math.Min(sourceStride, targetStride);

            for (int y = 0; y < height; ++y)
            {
                CopyMemory(targetPtr, sourcePtr, lineLength);
                sourcePtr = IntPtr.Add(sourcePtr, sourceStride);
                targetPtr = IntPtr.Add(targetPtr, targetStride);
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);
    }
}

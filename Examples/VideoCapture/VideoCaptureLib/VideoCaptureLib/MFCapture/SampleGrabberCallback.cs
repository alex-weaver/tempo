using MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib.MFCapture
{
    [ComVisible(true), Guid("a4f240c7-4861-4d86-b96e-06c8938e163d")]
    [ClassInterface(ClassInterfaceType.None)]
    public class SampleGrabberCallback : IMFSampleGrabberSinkCallback2
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

        private const int S_OK = 1;
        private Action<DataBuffer> onSample;

        public SampleGrabberCallback(Action<DataBuffer> onSample)
        {
            this.onSample = onSample;
        }

        public int OnClockPause(long hnsSystemTime)
        {
            return S_OK;
        }

        public int OnClockRestart(long hnsSystemTime)
        {
            return S_OK;
        }

        public int OnClockSetRate(long hnsSystemTime, float flRate)
        {
            return S_OK;
        }

        public int OnClockStart(long hnsSystemTime, long llClockStartOffset)
        {
            return S_OK;
        }

        public int OnClockStop(long hnsSystemTime)
        {
            return S_OK;
        }

        public int OnProcessSample(Guid guidMajorMediaType, int dwSampleFlags, long llSampleTime, long llSampleDuration, IntPtr pSampleBuffer, int dwSampleSize)
        {
            return S_OK;
        }

        public int OnProcessSampleEx(Guid guidMajorMediaType, int dwSampleFlags, long llSampleTime, long llSampleDuration, IntPtr pSampleBuffer, int dwSampleSize,
            IMFAttributes attributes)
        {
            if (onSample != null)
            {
                var buffer = new DataBuffer(dwSampleSize);
                CopyMemory(buffer.Data, pSampleBuffer, (uint)dwSampleSize);
                onSample(buffer);
            }

            return S_OK;
        }

        public int OnSetPresentationClock(IMFPresentationClock pPresentationClock)
        {
            return S_OK;
        }

        public int OnShutdown()
        {
            return S_OK;
        }
    }

}

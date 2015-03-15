using MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib.MFCapture
{
    public static class MFFunctions
    {
        public static int MFSetAttributeSize(IMFAttributes attributes, Guid key, uint width, uint height)
        {
            var packed = PackUint64(width, height);
            return attributes.SetUINT64(key, (long)packed);
        }


        public static int MFGetAttributeSize(IMFAttributes attributes, Guid key, out uint width, out uint height)
        {
            long longValue;
            int hResult = attributes.GetUINT64(key, out longValue);

            UnpackUint64((ulong)longValue, out width, out height);

            return hResult;
        }


        public static int MFGetAttributeRatio(IMFAttributes attributes, Guid key, out uint numerator, out uint denominator)
        {
            long longValue;
            int hResult = attributes.GetUINT64(key, out longValue);

            UnpackUint64((ulong)longValue, out numerator, out denominator);

            return hResult;
        }

        private static void UnpackUint64(UInt64 packed, out UInt32 high, out UInt32 low)
        {
            high = (UInt32)(packed >> 32);
            low = (UInt32)(packed);
        }

        private static UInt64 PackUint64(UInt32 high, UInt32 low)
        {
            return ((UInt64)high << 32) | low;
        }
    }

}

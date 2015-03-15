using MediaFoundation;
using MediaFoundation.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib.MFCapture
{
    public class FormatInfo
    {
        public readonly int Width;
        public readonly int Height;

        public readonly int FpsNumerator;
        public readonly int FpsDenominator;


        public FormatInfo(int width, int height, int fpsNumerator, int fpsDenominator)
        {
            this.Width = width;
            this.Height = height;
            this.FpsNumerator = fpsNumerator;
            this.FpsDenominator = fpsDenominator;
        }
    }

    public static class FormatEnum
    {
        public static IEnumerable<FormatInfo> EnumFormats(IMFMediaTypeHandler mtHandler)
        {
            var result = new List<FormatInfo>();

            int mtCount;
            MFError.ThrowExceptionForHR(mtHandler.GetMediaTypeCount(out mtCount));
            for (int i = 0; i < mtCount; ++i)
            {
                IMFMediaType mediaType;
                uint cmtwidth, cmtheight;
                uint fpsNum, fpsDenom;

                MFError.ThrowExceptionForHR(mtHandler.GetMediaTypeByIndex(i, out mediaType));
                MFError.ThrowExceptionForHR(MFFunctions.MFGetAttributeSize(mediaType, MFAttributesClsid.MF_MT_FRAME_SIZE, out cmtwidth, out cmtheight));
                MFError.ThrowExceptionForHR(MFFunctions.MFGetAttributeRatio(mediaType, MFAttributesClsid.MF_MT_FRAME_RATE, out fpsNum, out fpsDenom));

                result.Add(new FormatInfo((int)cmtwidth, (int)cmtheight, (int)fpsNum, (int)fpsDenom));

                Marshal.ReleaseComObject(mediaType);
            }

            return result;
        }

        public static IMFMediaType GetMediaType(IMFMediaTypeHandler mtHandler, FormatInfo formatInfo)
        {
            int mtCount;
            MFError.ThrowExceptionForHR(mtHandler.GetMediaTypeCount(out mtCount));
            for (int i = 0; i < mtCount; ++i)
            {
                IMFMediaType mediaType;
                MFError.ThrowExceptionForHR(mtHandler.GetMediaTypeByIndex(i, out mediaType));

                uint cmtwidth, cmtheight;
                MFError.ThrowExceptionForHR(MFFunctions.MFGetAttributeSize(mediaType, MFAttributesClsid.MF_MT_FRAME_SIZE, out cmtwidth, out cmtheight));

                uint fpsNum, fpsDenom;
                MFError.ThrowExceptionForHR(MFFunctions.MFGetAttributeRatio(mediaType, MFAttributesClsid.MF_MT_FRAME_RATE, out fpsNum, out fpsDenom));

                if (cmtwidth == formatInfo.Width &&
                    cmtheight == formatInfo.Height &&
                    fpsNum == formatInfo.FpsNumerator &&
                    fpsDenom == formatInfo.FpsDenominator)
                {
                    return mediaType;
                }
                else
                {
                    Marshal.ReleaseComObject(mediaType);
                }
            }

            return null;
        }
    }
}

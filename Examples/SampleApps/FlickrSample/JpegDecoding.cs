using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FlickrSample
{
    public static class JpegDecoding
    {
        public static BitmapFrame DecodeJpeg(byte[] data)
        {
            using (var jpegStream = new MemoryStream(data))
            {
                var decoder = new JpegBitmapDecoder(jpegStream,
                        BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.OnLoad);

                var frame = decoder.Frames[0];
                return frame;
            }
        }
    }
}

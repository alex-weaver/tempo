using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tempo;
using TwistedOak.Util;
using VideoCaptureLib;

namespace VideoTest
{
    public class WriteableBitmapPool
    {
        private ObjectPool<Tuple<int, int>, WriteableBitmap> pool;


        public WriteableBitmapPool(int maxObjectCount)
        {
            this.pool = new ObjectPool<Tuple<int, int>, WriteableBitmap>(
                size => new WriteableBitmap(size.Item1, size.Item2, 96, 96, PixelFormats.Bgr32, null),
                maxObjectCount);
        }

        public WriteableBitmap Allocate(int width, int height)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            return pool.Get(callingScope.lifetime, Tuple.Create(width, height));
        }

        public WriteableBitmap AllocateWith(VideoFrame frame)
        {
            var target = Allocate(frame.Width, frame.Height);

            target.Lock();

            BitmapHelpers.Copy(frame.ImageData.Data, frame.Stride, target.BackBuffer, target.BackBufferStride, frame.Height);

            target.AddDirtyRect(new Int32Rect(0, 0, target.PixelWidth, target.PixelHeight));
            target.Unlock();

            return target;
        }

    }
}

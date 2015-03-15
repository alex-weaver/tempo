using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tempo;

namespace VideoCaptureLib
{
    public class VideoFrame : RefCountedSafe
    {
        private int width, height;

        private int stride;
        private int bitsPerPixel;
        private DataBuffer imageData;

        public int Width { get { ThrowIfDestroyed(); return width; } }
        public int Height { get { ThrowIfDestroyed(); return height; } }

        public int Stride { get { ThrowIfDestroyed(); return stride; } }
        public int BitsPerPixel { get { ThrowIfDestroyed(); return bitsPerPixel; } }
        public DataBuffer ImageData { get { ThrowIfDestroyed(); return imageData; } }

        public VideoFrame(int width, int height, int stride, int bitsPerPixel, DataBuffer buffer)
        {
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.bitsPerPixel = bitsPerPixel;
            this.imageData = buffer;
        }

        protected override void Destroy()
        {
            this.imageData.Dispose();
        }
    }
}

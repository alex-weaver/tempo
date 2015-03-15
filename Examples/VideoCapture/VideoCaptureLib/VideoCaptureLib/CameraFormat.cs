using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib
{
    public class CameraFormat
    {
        public readonly int Width;
        public readonly int Height;

        public CameraFormat(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}

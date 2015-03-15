using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tempo;
using TwistedOak.Util;
using MediaFoundation;
using VideoCaptureLib.MFCapture;
using VideoCaptureLib.Util;

namespace VideoCaptureLib
{
    public class CaptureDevice : ContinuousObject
    {
        private ComObject<IMFActivate> deviceActivate;
        private RequestBroker<VideoRequest, VideoRequest> requests;
        private ICellRead<VideoFrame> results;
        private IEnumerable<FormatInfo> allFormats;

        internal CaptureDevice(IMFActivate deviceActivate)
        {
            this.deviceActivate = new ComObject<IMFActivate>(deviceActivate);

            this.FriendlyName = DeviceEnumeration.GetFriendlyName(deviceActivate);
            this.SymbolicLink = DeviceEnumeration.GetSymbolicLink(deviceActivate);


            allFormats = CaptureBuilder.EnumFormats(deviceActivate);
            var formatSet = new HashSet<Tuple<int, int>>(
                allFormats
                .Select(x => Tuple.Create(x.Width, x.Height)));

            this.VideoFormats = formatSet.Select(x => new CameraFormat(x.Item1, x.Item2)).ToArray();

            InitScope();
        }

        protected override void WhileAlive()
        {
            this.requests = new RequestBroker<VideoRequest, VideoRequest>(
                null,
                allRequests => allRequests.MaxObject(x => x.Format.Width * x.Format.Height));

            this.results = requests.Aggregate.WithLatest(req =>
            {
                var callingScope = CurrentThread.CurrentContinuousScope();

                var result = new MemoryCell<VideoFrame>(null);

                if (req != null)
                {
                    var actualFormat = allFormats
                        .Where(x => x.Width == req.Format.Width && x.Height == req.Format.Height)
                        .MaxObject(x => x.FpsNumerator / (double)x.FpsDenominator);

                    CaptureBuilder.Init(callingScope.lifetime, deviceActivate.Ptr, actualFormat, buffer =>
                    {
                        var stride = req.Format.Width * 4;
                        var frame = new VideoFrame(req.Format.Width, req.Format.Height, stride, 32, buffer);
                        callingScope.ScheduleSequentialBlock(() =>
                        {
                            result.Cur = frame;
                            frame.Release();
                        });
                    });
                }

                return result;
            });
        }

        protected override void Destroy()
        {
            base.Destroy();
            deviceActivate.Dispose();            
        }

        public string FriendlyName { get; private set; }
        public string SymbolicLink { get; private set; }
        public CameraFormat[] VideoFormats { get; private set; }


        public ICellRead<VideoFrame> ApplyVideoRequest(CameraFormat format)
        {
            requests.ApplyConstRequest(new VideoRequest(format));
            return results;
        }


        private class VideoRequest
        {
            public readonly CameraFormat Format;

            public VideoRequest(CameraFormat format)
            {
                this.Format = format;
            }

            public override bool Equals(object obj)
            {
                var o = obj as VideoRequest;
                if (o == null) return false;

                return this.Format.Equals(o.Format);
            }

            public override int GetHashCode()
            {
                return Format.GetHashCode();
            }
        }
    }
}

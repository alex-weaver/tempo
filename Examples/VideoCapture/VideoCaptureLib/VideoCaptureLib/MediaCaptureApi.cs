using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Tempo;

namespace VideoCaptureLib
{
    public class MediaCaptureApi
    {
        private readonly ListCell<CaptureDevice> _devices;

        public IListCellRead<CaptureDevice> Devices { get { return _devices; } }


        public MediaCaptureApi()
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            this._devices = new ListCell<CaptureDevice>();

            MFCapture.DeviceList.KeepSync(callingScope, _devices);
        }
    }
}

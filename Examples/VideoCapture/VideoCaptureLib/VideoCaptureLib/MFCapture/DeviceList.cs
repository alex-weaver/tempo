using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tempo;
using VideoCaptureLib.DeviceNotification;

namespace VideoCaptureLib.MFCapture
{
    public static class DeviceList
    {
        public static void KeepSync(TemporalScope scope, ListCell<CaptureDevice> devices)
        {
            devices.Clear();

            scope.ScheduleSequentialBlock(() => RefreshDevices(devices));

            DeviceWatcher.WatchVideoDevices(scope.lifetime,
                devName =>
                {
                    scope.ScheduleSequentialBlock(() => RefreshDevices(devices));
                },
                devName =>
                {
                    scope.ScheduleSequentialBlock(() => RefreshDevices(devices));
                });
        }

        private static void RefreshDevices(ListCell<CaptureDevice> devices)
        {
            var connectedDevices =
                DeviceEnumeration.EnumVideoDevices(null)
                .ToDictionary(x => DeviceEnumeration.GetSymbolicLink(x).ToLower());

            // remove any devices that are in the 'devices' list but are no longer enumerated by EnumVideoDevices
            for(int i = 0; i < devices.Cur.Count(); ++i)
            {
                if(!connectedDevices.ContainsKey(devices[i].SymbolicLink.ToLower()))
                {
                    devices.RemoveAt(i);
                    --i;
                }
            }


            // Add any new devices that have appeared but are not yet in the 'devices' list. Also release
            // any IMFActivate objects returned from EnumVideoDevices which are not retained by a new CaptureDevice object
            var symlinksInList = new HashSet<string>(devices.Cur.Select(x => x.SymbolicLink.ToLower()));
            foreach (var connected in connectedDevices)
            {
                if(!symlinksInList.Contains(connected.Key))
                {
                    devices.Add(new CaptureDevice(connected.Value));
                }
                else
                {
                    Marshal.ReleaseComObject(connected.Value);
                }
            }
        }
    }
}

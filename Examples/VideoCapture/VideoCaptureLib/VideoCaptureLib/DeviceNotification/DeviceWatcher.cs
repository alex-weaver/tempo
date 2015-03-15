using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TwistedOak.Util;

namespace VideoCaptureLib.DeviceNotification
{
    public static class DeviceWatcher
    {
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;

        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private const int WM_DEVICECHANGE = 0x219;

        private const int DBT_DEVTYP_DEVICEINTERFACE = 5;

        private static readonly Guid KSCATEGORY_CAPTURE = new Guid("{65E8773D-8F56-11D0-A3B9-00A0C9223196}");
        private static readonly Guid KSCATEGORY_AUDIO = new Guid("{6994AD04-93EF-11D0-A3CC-00A0C9223196}");

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, uint Flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterDeviceNotification(IntPtr Handle);


        public static void WatchVideoDevices(Lifetime lifetime, Action<string> added, Action<string> removed)
        {
            var deviceNotifyWindow = new InvisibleWindow((msg, wParam, lParam) =>
            {
                if (msg == WM_DEVICECHANGE && wParam.ToInt32() == DBT_DEVICEARRIVAL)
                {
                    var name = GetDeviceName(lParam);
                    if(name != null)
                    {
                        added(name);
                    }
                }
                else if(msg == WM_DEVICECHANGE && wParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
                {
                    var name = GetDeviceName(lParam);
                    if (name != null)
                    {
                        removed(name);
                    }
                }
            });

            RegisterForNotification(lifetime, deviceNotifyWindow.Handle, KSCATEGORY_CAPTURE);

            lifetime.WhenDead(() => deviceNotifyWindow.DestroyHandle());
        }

        private static string GetDeviceName(IntPtr broadcastStruct)
        {
            var header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(broadcastStruct, typeof(DEV_BROADCAST_HDR));

            if(header.dbch_DeviceType != DBT_DEVTYP_DEVICEINTERFACE)
            {
                return null;
            }

            var deviceInterface = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(broadcastStruct, typeof(DEV_BROADCAST_DEVICEINTERFACE));


            return deviceInterface.dbcc_name;
        }

        private static void RegisterForNotification(Lifetime lifetime, IntPtr windowHandle, Guid deviceClass)
        {
            IntPtr mem = IntPtr.Zero;
            try
            {
                var di = new DEV_BROADCAST_DEVICEINTERFACE();
                di.dbcc_size = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE));
                di.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                di.dbcc_classguid = deviceClass;
                
                mem = Marshal.AllocHGlobal(di.dbcc_size);
                Marshal.StructureToPtr(di, mem, false);
                IntPtr hDevNotify = RegisterDeviceNotification(windowHandle, mem, DEVICE_NOTIFY_WINDOW_HANDLE);

                lifetime.WhenDead(() => UnregisterDeviceNotification(hDevNotify));
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoCaptureLib.DeviceNotification
{
    internal delegate void WndProcDelegate(int msg, IntPtr wParam, IntPtr lParam);

    internal class InvisibleWindow : NativeWindow
    {
        private WndProcDelegate callback;

        public InvisibleWindow(WndProcDelegate callback)
        {
            this.callback = callback;

            var createParams = new CreateParams();
            createParams.ClassName = "Message"; // A built-in class for invisible message windows
            CreateHandle(createParams);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            callback(m.Msg, m.WParam, m.LParam);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tempo.Wpf;

namespace AllCamerasSample
{
    public partial class App : Application
    {
        public App()
        {
            TempoApp.Init(this, true, WhileRunning);
        }

        private void WhileRunning()
        {
            var window = new MainWindow();
            window.Show();
        }
    }
}

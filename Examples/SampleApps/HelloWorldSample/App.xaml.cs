using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tempo;
using Tempo.Wpf;

namespace HelloWorldSample
{
    public partial class App : Application
    {
        public App()
        {
            TempoApp.Init(this, true, WhileRunning);
        }

        private void WhileRunning()
        {
            var count = new MemoryCell<int>(0);
            var list = new ListCell<string>();

            var window = new MainWindow();
            window.Show();
            var buttonPressed = PropertyAdapter.Read<bool>(window.button, Button.IsPressedProperty);
            var labelWrite = PropertyAdapter.Write<string>(window.label, TextBlock.TextProperty);

            var labelText = count.Select(x => x.ToString() + " clicks");
            labelText.Bind(labelWrite);

            list.Bind(window.listView.Items);

            buttonPressed.WhenTrue(() =>
                {
                    count.Cur++;
                    list.Add("Hello World");
                });
        }
    }
}

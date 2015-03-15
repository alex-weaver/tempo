# Quickstart

To get up and running quickly with Tempo, you can set up a fresh WPF application by
doing the following:

1. Create a new WPF project
2. Add references to Tempo.dll, Tempo.Wpf.dll and Lifetime.dll; pre-built versions of these
files are included in the bin\ directory in the Tempo repository.
3. Open App.xaml and remove the StartupUri attribute.
4. Edit App.xaml.cs to include the following initialization code. To compile, this will
require adding using statements for the Tempo and Tempo.Wpf namespaces.

```
public partial class App : Application
{
    public App()
    {
        TempoApp.Init(this, true, WhileRunning);
    }

    private void WhileRunning()
    {
        // This is the top-level reactive scope for the application

        var window = new MainWindow();
        window.Show();
    }
}
```

Running this program should display a blank window.

See the Examples folder in the repository for more detail on integrating with WPF.


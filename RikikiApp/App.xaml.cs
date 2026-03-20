using Microsoft.Maui.ApplicationModel;
using RikikiApp.Data;
using System.Diagnostics;
using System.Globalization;

namespace RikikiApp { 

public partial class App : Application
{
    private readonly LocalDB _localDb;

    public App(LocalDB localDb)
    {
        InitializeComponent();
        _localDb = localDb;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();
        var window = new Window(shell);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await _localDb.InitAsync();
                Debug.WriteLine("✅ DB init OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ DB init ERROR:\n" + ex);
            }
        });

        return window;
    }


}


}
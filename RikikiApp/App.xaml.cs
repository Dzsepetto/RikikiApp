using Microsoft.Maui.ApplicationModel;
using RikikiApp.Data;
using RikikiApp.Views;
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

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Debug.WriteLine($"🔥 UNHANDLED: {e.ExceptionObject}");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Debug.WriteLine($"🔥 TASK ERROR: {e.Exception}");
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var mainPage = Handler.MauiContext.Services.GetRequiredService<MainLayoutPage>();

            var window = new Window(mainPage);

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
using RikikiApp.Views;

namespace RikikiApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
            Routing.RegisterRoute(nameof(GameSetupPage), typeof(GameSetupPage));
            Routing.RegisterRoute(nameof(GamePlayPage), typeof(GamePlayPage));
        }
    }
}

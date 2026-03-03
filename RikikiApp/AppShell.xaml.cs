using RikikiApp.Views;

namespace RikikiApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
            Routing.RegisterRoute(nameof(GameSetupPage), typeof(GameSetupPage));
        }
    }
}

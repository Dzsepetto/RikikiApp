using RikikiApp.Data;
using RikikiApp.Models;
namespace RikikiApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OpenGamesView(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(GamePage));
        }
        private async void OnDbTestClicked(object sender, EventArgs e)
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;

            if (services == null)
            {
                await DisplayAlert("Error", "Services not available", "OK");
                return;
            }

            var localDb = services.GetRequiredService<LocalDB>();
            var db = await localDb.GetAsync();

            await db.InsertAsync(new Game
            {
                Name = "Test game",
                CreatedAt = DateTime.UtcNow,
                ScoringVersion = "classic-v1"
            });

            var count = await db.Table<Game>().CountAsync();
            await DisplayAlert("DB OK", $"Games in DB: {count}", "OK");
        }
    }
}

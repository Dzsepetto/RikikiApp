using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Infrastructure.Persistance;
using RikikiApp.Features.Games.Domain.Scoring;
using RikikiApp.Features.Games.Views;
using RikikiApp.Features.Games.ViewModels;

namespace RikikiApp.Features.Main.ViewModels
{
    public partial class MainViewVM : ObservableObject
    {
        private readonly LocalDB _localDb;
        private readonly NavigationService _nav;

        public MainViewVM(LocalDB localDb, NavigationService nav)
        {
            _localDb = localDb;
            _nav = nav;
        }

        [RelayCommand]
        private async Task OpenGamesView()
        {
            await _nav.PushWithLoading<GameView, GameViewVM>(vm => vm.InitAsync());
        }
        [RelayCommand]
        private async Task OpenInfoView()
        {
            await _nav.PushWithLoading<GameView, GameViewVM>(vm => vm.InitAsync());
        }
        [RelayCommand]
        private async Task DbTest()
        {
            var db = await _localDb.GetAsync();

            await db.InsertAsync(new Game
            {
                Name = "Test game",
                CreatedAt = DateTime.UtcNow,
                ScoringType = ScoringType.Basic
            });

            var count = await db.Table<Game>().CountAsync();

            await Application.Current.MainPage.DisplayAlertAsync(
                "DB OK",
                $"Games in DB: {count}",
                "OK");
        }
    }
}
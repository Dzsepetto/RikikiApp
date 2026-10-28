using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using RikikiApp.Core.Popups;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Features.Games.Domain.Scoring;

using GameEntity = RikikiApp.Features.Games.Domain.Entities.Game;
using RikikiApp.Repositories.Interfaces;

namespace RikikiApp.Features.Games.ViewModels.Popups;

public partial class AddGamePopupVM : ObservableObject, IPopupResults<GameEntity>, IPopupAware
{
    private readonly IGameRepository _games;

    private readonly TaskCompletionSource<GameEntity?> _tcs = new();
    public Task<GameEntity?> ResultTask => _tcs.Task;

    //UI bindok
    [ObservableProperty]
    private string gameName;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private ScoringType selectedScoringType;

    public List<ScoringType> ScoringTypes { get; } =
        Enum.GetValues(typeof(ScoringType)).Cast<ScoringType>().ToList();

    //popup referencia
    public Popup? PopupInstance { get; set; }

    public AddGamePopupVM(IGameRepository games)
    {
        _games = games;

        SelectedScoringType = ScoringType.Basic;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (IsSaving || _tcs.Task.IsCompleted)
            return;

        if (string.IsNullOrWhiteSpace(GameName))
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;

            if (page != null)
                await page.DisplayAlertAsync(
                    "Error",
                    "Name required",
                    "OK");

            return;
        }

        IsSaving = true;

        try
        {
            var game = new GameEntity
            {
                Name = GameName.Trim(),
                CreatedAt = DateTime.UtcNow,
                ScoringType = SelectedScoringType,
                Status = GameStatus.Setup
            };

            game = await _games.UpsertAsync(game);

            _tcs.TrySetResult(game);

            await Close();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        if (IsSaving || _tcs.Task.IsCompleted)
            return;

        _tcs.TrySetResult(null);

        await Close();
    }

    private async Task Close()
    {
        if (PopupInstance != null)
            await PopupInstance.CloseAsync();
    }
}
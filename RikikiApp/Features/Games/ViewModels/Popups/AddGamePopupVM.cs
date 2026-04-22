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

    //SAVE
    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(GameName))
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", "Name required", "OK");
            return;
        }

        var game = new GameEntity
        {
            Name = GameName.Trim(),
            CreatedAt = DateTime.UtcNow,
            ScoringType = SelectedScoringType,
            Status = GameStatus.Setup
        };

        game = await _games.UpsertAsync(game);

        _tcs.SetResult(game);

        await Close();
    }

    //CANCEL
    [RelayCommand]
    private async Task Cancel()
    {
        _tcs.SetResult(null);
        await Close();
    }

    private async Task Close()
    {
        if (PopupInstance != null)
            await PopupInstance.CloseAsync();
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;

namespace RikikiApp.ViewModel.Popups;

public partial class AddGamePopupVM : ObservableObject, IPopupResults<Game>, IPopupAware
{
    private readonly IGameRepository _games;

    private readonly TaskCompletionSource<Game?> _tcs = new();
    public Task<Game?> ResultTask => _tcs.Task;

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

        var game = new Game
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
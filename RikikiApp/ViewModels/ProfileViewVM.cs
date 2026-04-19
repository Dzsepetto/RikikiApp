using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using RikikiApp.ViewModels.Components;
using RikikiApp.Views.Components;
using System.Diagnostics;

namespace RikikiApp.ViewModels;

public partial class ProfileViewVM : ObservableObject
{
    private readonly IPlayerRepository _players;
    private readonly IUserRepository _users;
    private readonly NavigationService _nav;
    private readonly UserSessionService _session;
    private readonly IGameRepository _games;

    [ObservableProperty]
    private string editableUserName = "";

    [ObservableProperty]
    private bool isEditingName = false;

    public bool IsNameReadOnly => !IsEditingName;
    public string EditButtonText => IsEditingName ? "Save player" : "Edit player";

    public bool IsLoggedIn => _session.IsLoggedIn;
    public string ProfileImage => IsLoggedIn ? "user.png" : "default_user.png";

    [ObservableProperty]
    private int wins;

    [ObservableProperty]
    private int losses;

    [ObservableProperty]
    private int totalGames;

    public string NeedsToLoginTxt => IsLoggedIn ? "" : "Please log in to strart tracking your real points";

    public ProfileViewVM(
        IPlayerRepository players,
        IUserRepository users,
        NavigationService nav,
        UserSessionService session,
        IGameRepository game)
    {
        _players = players;
        _users = users;
        _nav = nav;
        _session = session;
        _games = game;

        EditableUserName = _session.CurrentUser?.Name ?? "Player";
    }
    public async Task InitAsync()
    {
        try
        {
            Debug.WriteLine("ProfileViewVM.InitAsync START");

            await LoadStatsAsync();

            Debug.WriteLine("ProfileViewVM.InitAsync END");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ProfileViewVM.InitAsync ERROR: {ex}");
        }
    }
    partial void OnIsEditingNameChanged(bool value)
    {
        OnPropertyChanged(nameof(EditButtonText));
        OnPropertyChanged(nameof(IsNameReadOnly));
    }

    [RelayCommand]
    private async Task EditOrSavePlayer()
    {
        if (!IsEditingName)
        {
            IsEditingName = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(EditableUserName))
            return;

        var currentUser = _session.CurrentUser;
        if (currentUser == null)
            return;

        var newName = EditableUserName.Trim();

        currentUser.Name = newName;
        await _users.UpdateAsync(currentUser);

        var localPlayer = await _players.GetByUserIdAsync(currentUser.Id);
        if (localPlayer != null)
        {
            localPlayer.Name = newName;
            await _players.UpdateAsync(localPlayer);
        }

        _session.SetCurrentUser(currentUser);

        IsEditingName = false;

        OnPropertyChanged(nameof(NeedsToLoginTxt));
        OnPropertyChanged(nameof(ProfileImage));
    }

    public async Task LoadStatsAsync()
    {
        var currentUser = _session.CurrentUser;
        if (currentUser == null)
        {
            Wins = 0;
            Losses = 0;
            TotalGames = 0;
            return;
        }

        var player = await _players.GetByUserIdAsync(currentUser.Id);
        if (player == null)
        {
            Wins = 0;
            Losses = 0;
            TotalGames = 0;
            return;
        }

        var games = await _games.GetByPlayerIdAsync(player.Id);

        TotalGames = games.Count();
        Wins = 0;//games.Count(g => g.WinnerPlayerId == player.Id);
        Losses = 0;//TotalGames - Wins;
    }

    [RelayCommand]
    private async Task MoveToPlayers()
    {
        await _nav.PushWithLoading<ManagePlayersView, ManagePlayersVM>(
            async vm => await vm.InitAsync());
    }
}
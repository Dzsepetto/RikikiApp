using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Repositories;
using RikikiApp.Services;
using RikikiApp.Views.Components;
using RikikiApp.ViewModels.Components;

namespace RikikiApp.ViewModels;

public partial class ProfileViewVM : ObservableObject
{
    private readonly IPlayerRepository _players;
    private readonly IUserRepository _users;
    private readonly NavigationService _nav;
    private readonly UserSessionService _session;

    [ObservableProperty]
    private string editableUserName = "";

    public bool IsLoggedIn => _session.IsLoggedIn;

    public string ProfileImage =>
        IsLoggedIn ? "user.png" : "default_user.png";

    public int Wins => 0;
    public int Losses => 0;
    public int TotalGames => 0;

    public string NeedsToLoginTxt => IsLoggedIn ? "" : "Please log in";

    public ProfileViewVM(
        IPlayerRepository players,
        IUserRepository users,
        NavigationService nav,
        UserSessionService session)
    {
        _players = players;
        _users = users;
        _nav = nav;
        _session = session;

        EditableUserName = _session.CurrentUser?.Name ?? "Player";
    }

    [RelayCommand]
    private async Task SaveName()
    {
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
        OnPropertyChanged(nameof(NeedsToLoginTxt));
        OnPropertyChanged(nameof(ProfileImage));
    }

    [RelayCommand]
    private async Task MoveToPlayers()
    {
        await _nav.PushWithLoading<ManagePlayersView, ManagePlayersVM>(async vm => await vm.InitAsync());
    }
}
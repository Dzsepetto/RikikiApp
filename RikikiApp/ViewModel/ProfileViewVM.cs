using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using RikikiApp.ViewModel.Components;
using RikikiApp.Views.Components;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RikikiApp.ViewModels;

public partial class ProfileViewVM : ObservableObject
{
    private readonly UserSessionService _session;

    public bool IsLoggedIn => _session.IsLoggedIn;

    public string UserName =>
        IsLoggedIn
        ? _session.CurrentUser!.Name
        : "Guest";


    public string ProfileImage =>
        IsLoggedIn
        ? "user.png" // majd később user-specifikus
        : "default_user.png";

    public int Wins = 0; //_stats?.Wins ?? 0;
    public int Losses = 0; // _stats?.Losses ?? 0;
    public int TotalGames = 0; // _stats?.TotalGames ?? 0;
    private readonly NavigationService _nav;
   private string NeedsToLoginTxt => IsLoggedIn ? "" : "Please log in";

    public ProfileViewVM(IPlayerRepository players, NavigationService nav, UserSessionService session)
    {
        _nav = nav;
        _session = session;
    }
   
    [RelayCommand]
    private async Task MoveToPlayers()
    {
        await _nav.PushWithLoading<ManagePlayersView, ManagePlayersVM>();
    }
}

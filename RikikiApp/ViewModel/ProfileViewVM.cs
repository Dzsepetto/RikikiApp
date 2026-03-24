using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RikikiApp.ViewModels;

public partial class ProfileViewVM : ObservableObject
{
    private readonly IPlayerRepository _players;

    [ObservableProperty]
    private string needsToLoginTxt = "For a better experience, login...";

    [ObservableProperty]
    private string newPlayerName;

    public ObservableCollection<Player> Players { get; } = new();

    public ProfileViewVM(IPlayerRepository players)
    {
        _players = players;
    }
    public async Task Initialize()
    {
        await LoadPlayers();
    }
    public async Task LoadPlayers()
    {
        Players.Clear();

        var list = await _players.GetAllAsync();
        foreach (var p in list)
            Players.Add(p);
    }

    [RelayCommand]
    private async Task AddPlayer()
    {
        if (string.IsNullOrWhiteSpace(NewPlayerName))
            return;

        var player = new Player { Name = NewPlayerName };

        await _players.AddAsync(player);

        NewPlayerName = "";
        await LoadPlayers();
    }
}
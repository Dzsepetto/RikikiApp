using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using RikikiApp.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

public partial class AddPlayerPopupVM : ObservableObject, IInitializable, IPopupResults<List<string>>, IPopupAware
{
    private readonly IPlayerRepository _players;

    private readonly TaskCompletionSource<List<string>?> _tcs = new();
    public Task<List<string>?> ResultTask => _tcs.Task;
    public ObservableCollection<PlayerItemVM> Players { get; } = new();

    [ObservableProperty]
    private string newPlayerName;

    [ObservableProperty]
    private bool isAddMode;

    [ObservableProperty]
    private string searchText;

    public string ModeText => IsAddMode ? "Add new player" : "Select players";

    private List<PlayerItemVM> _allPlayers = new();


    public AddPlayerPopupVM(IPlayerRepository players)
    {
        _players = players;
    }

    public async Task InitAsync()
    {
        await LoadPlayers();
    }
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }
    private async Task LoadPlayers()
    {
        Players.Clear();

        var list = await _players.GetAllAsync();

        _allPlayers = list
            .Select(p => new PlayerItemVM(p))
            .ToList();

        ApplyFilter();
    }
    private void ApplyFilter()
    {
        Players.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allPlayers
            : _allPlayers.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var p in filtered)
            Players.Add(p);
    }

    // SELECT EXISTING
    [RelayCommand]
    private async Task SelectPlayer(Player player)
    {
        if(!_tcs.Task.IsCompleted)
            _tcs.SetResult(new List<string> { player.Name });
        await Close();
    }

    [RelayCommand]
    private void TogglePlayer(PlayerItemVM player)
    {
        player.IsSelected = !player.IsSelected;
    }
    partial void OnIsAddModeChanged(bool value)
    {
        OnPropertyChanged(nameof(ModeText));
    }
    // ADD NEW
    [RelayCommand]
    private async Task Add()
    {
        if (IsAddMode)
        {
            var name = NewPlayerName?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                return;

            var existing = await _players.GetAllAsync();
            var player = existing.FirstOrDefault(p => p.Name == name);

            if (player == null)
            {
                player = new Player { Name = name };
                await _players.AddAsync(player);
            }

            if (!_tcs.Task.IsCompleted)
                _tcs.SetResult(new List<string> { player.Name });

            await Close();
            return;
        }

        // 👉 SELECT MODE
        var selected = Players
            .Where(p => p.IsSelected)
            .Select(p => p.Name)
            .ToList();

        if (!selected.Any())
            return;

        if (!_tcs.Task.IsCompleted)
            _tcs.SetResult(selected);

        await Close();
    }

    // CANCEL
    [RelayCommand]
    private async Task Cancel()
    {
        if (!_tcs.Task.IsCompleted)
            _tcs.SetResult(null);
        await Close();
    }

    // popup referencia
    public Popup? PopupInstance { get; set; }

    private async Task Close()
    {
        if (PopupInstance != null)
            await PopupInstance.CloseAsync();
    }
}
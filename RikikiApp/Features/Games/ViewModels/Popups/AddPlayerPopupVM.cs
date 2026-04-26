using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Core.Abstractions;
using RikikiApp.Core.Popups;
using RikikiApp.Core.Session;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Features.Games.ViewModels.UiWrappers;
using RikikiApp.Repositories.Interfaces;
using System.Collections.ObjectModel;

namespace RikikiApp.Features.Games.ViewModels.Popups;

public partial class AddPlayerPopupVM : ObservableObject, IInitializable, IPopupResults<List<string>>, IPopupAware
{
    private readonly IPlayerRepository _players;
    private readonly UserSessionService _session;

    private readonly TaskCompletionSource<List<string>?> _tcs = new();
    public Task<List<string>?> ResultTask => _tcs.Task;
    public ObservableCollection<PlayerItemVM> Players { get; } = new();
    public HashSet<int> ExcludedPlayerIds { get; set; } = new();
    public HashSet<string> ExcludedNames { get; set; } = new();

    [ObservableProperty]
    private string newPlayerName;

    [ObservableProperty]
    private bool isAddMode;

    [ObservableProperty]
    private string searchText;

    public string ModeText => IsAddMode ? "Add new player" : "Select players";

    private List<PlayerItemVM> _allPlayers = new();


    public AddPlayerPopupVM(
        IPlayerRepository players,
        UserSessionService session)
    {
        _players = players;
        _session = session;
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
        var currentUserId = _session.CurrentUser?.Id;

        _allPlayers = list
            .Where(p => p.UserId != currentUserId)
            .Select(p =>
            {
                var vm = new PlayerItemVM(p);

                var isById = ExcludedPlayerIds.Contains(p.Id);
                var isByName = ExcludedNames.Contains(p.Name.Trim().ToLower());

                if (isById || isByName)
                    vm.IsDisabled = true;

                return vm;
            })
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


    [RelayCommand]
    private async Task SelectPlayer(Player player)
    {
        if (ExcludedPlayerIds.Contains(player.Id))
            return;

        if (!_tcs.Task.IsCompleted)
            _tcs.SetResult(new List<string> { player.Name });

        await Close();
    }

    [RelayCommand]
    private void TogglePlayer(PlayerItemVM player)
    {
        if (player.IsDisabled)
            return;

        player.IsSelected = !player.IsSelected;
    }
    partial void OnIsAddModeChanged(bool value)
    {
        OnPropertyChanged(nameof(ModeText));
    }
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
    [RelayCommand]
    private async Task Cancel()
    {
        if (!_tcs.Task.IsCompleted)
            _tcs.SetResult(null);
        await Close();
    }
    public Popup? PopupInstance { get; set; }

    private async Task Close()
    {
        if (PopupInstance != null)
            await PopupInstance.CloseAsync();
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using RikikiApp.Core.Abstractions;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Repositories.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RikikiApp.Features.Profile.ViewModels.Components
{
    public partial class ManagePlayersVM : ObservableObject, IInitializable
    {
        private readonly NavigationService _nav;
        private readonly IPlayerRepository _players;

        [ObservableProperty]
        private string newPlayerName = "";

        public ObservableCollection<Player> Players { get; } = new();

        public ManagePlayersVM(
            NavigationService nav,
            IPlayerRepository players)
        {
            _nav = nav;
            _players = players;
        }

        public async Task InitAsync()
        {
            await LoadPlayers();
            Debug.WriteLine("Manageplayersvm loaded.");
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

            var player = new Player
            {
                Name = NewPlayerName.Trim()
            };

            await _players.AddAsync(player);

            NewPlayerName = "";
            await LoadPlayers();
        }

        [RelayCommand]
        private async Task EditPlayer(Player player)
        {
            if (player == null)
                return;

            var page = Application.Current?
                .Windows.FirstOrDefault()?.Page;

            if (page == null)
                return;

            string? newName = await page.DisplayPromptAsync(
                "Edit player",
                "Enter the new player name:",
                accept: "Save",
                cancel: "Cancel",
                placeholder: "Player name",
                initialValue: player.Name
            );

            if (string.IsNullOrWhiteSpace(newName))
                return;

            newName = newName.Trim();

            if (newName == player.Name)
                return;

            player.Name = newName;

            await _players.UpdateAsync(player);

            await LoadPlayers();
        }

        [RelayCommand]
        private async Task DeletePlayer(Player player)
        {
            if (player == null)
                return;

            await _players.DeleteAsync(player.Id);

            Players.Remove(player);
        }

        [RelayCommand]
        private async Task Back()
        {
            await _nav.Pop();
        }
    }
}
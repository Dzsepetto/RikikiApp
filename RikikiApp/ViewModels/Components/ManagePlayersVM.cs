using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace RikikiApp.ViewModels.Components
{
    public partial class ManagePlayersVM : ObservableObject, IInitializable
    {
        private readonly NavigationService _nav;
        private readonly IPlayerRepository _players;


        [ObservableProperty]
        private string newPlayerName;

        public ObservableCollection<Player> Players { get; } = new();


        public ManagePlayersVM(NavigationService nav, IPlayerRepository players) {
            
            _nav = nav;
            _players = players;
        }
        public async Task InitAsync()
        {
            await LoadPlayers();
            Debug.WriteLine("Manageplayersvm loaded. ");
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

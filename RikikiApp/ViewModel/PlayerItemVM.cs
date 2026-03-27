using CommunityToolkit.Mvvm.ComponentModel;
using RikikiApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.ViewModel
{
    public partial class PlayerItemVM : ObservableObject
    {
        public Player Model { get; }

        public int Id => Model.Id;
        public string Name => Model.Name;

        [ObservableProperty]
        private bool isSelected;

        public PlayerItemVM(Player player)
        {
            Model = player;
        }
    }
}

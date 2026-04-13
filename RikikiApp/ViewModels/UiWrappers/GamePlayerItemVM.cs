using CommunityToolkit.Mvvm.ComponentModel;
using RikikiApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.ViewModels.UiWrappers
{
    public partial class GamePlayerItemVM : ObservableObject
    {
        public GamePlayer Model { get; }

        public GamePlayerItemVM(GamePlayer model)
        {
            Model = model;
        }
        public int Id => Model.Id;

        public int GameId => Model.GameId;

        public int? PlayerId => Model.PlayerId;

        public int SeatOrder
        {
            get => Model.SeatOrder;
            set
            {
                Model.SeatOrder = value;
                OnPropertyChanged();
            }
        }

        public string GuestName
        {
            get => Model.GuestName;
            set
            {
                Model.GuestName = value;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        private bool isDropTarget;

    }
}

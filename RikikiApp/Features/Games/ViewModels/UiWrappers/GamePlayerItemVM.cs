using CommunityToolkit.Mvvm.ComponentModel;
using RikikiApp.Features.Games.Domain.Entities;


namespace RikikiApp.Features.Games.ViewModels.UiWrappers
{
    public partial class GamePlayerItemVM : ObservableObject
    {
        private readonly int? _localPlayerId;

        public GamePlayer Model { get; }

        public GamePlayerItemVM(GamePlayer model, int? localPlayerId)
        {
            Model = model;
            _localPlayerId = localPlayerId;
        }

        public int Id => Model.Id;
        public int GameId => Model.GameId;
        public int? PlayerId => Model.PlayerId;

        public bool IsLocalPlayer => PlayerId.HasValue && _localPlayerId == PlayerId.Value;
        public bool CanDelete => !IsLocalPlayer;

        public int SeatOrder
        {
            get => Model.SeatOrder;
            set
            {
                if (Model.SeatOrder != value)
                {
                    Model.SeatOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GuestName
        {
            get => Model.GuestName;
            set
            {
                if (Model.GuestName != value)
                {
                    Model.GuestName = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        private bool isDropTarget;
    }
}
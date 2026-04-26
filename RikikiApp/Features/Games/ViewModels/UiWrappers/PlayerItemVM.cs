using CommunityToolkit.Mvvm.ComponentModel;
using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Features.Games.ViewModels.UiWrappers
{
    public partial class PlayerItemVM : ObservableObject
    {
        public Player Model { get; }

        public int Id => Model.Id;
        public string Name => Model.Name;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private bool isDisabled;

        public PlayerItemVM(Player model)
        {
            Model = model;
        }
    }
}

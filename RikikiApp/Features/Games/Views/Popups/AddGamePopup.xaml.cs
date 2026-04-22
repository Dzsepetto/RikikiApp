using CommunityToolkit.Maui.Views;
using RikikiApp.Repositories.Interfaces;

namespace RikikiApp.Features.Games.Views.Popups;

public partial class AddGamePopup : Popup
{
    public AddGamePopup(IGameRepository games)
    {
        InitializeComponent();
    }
}
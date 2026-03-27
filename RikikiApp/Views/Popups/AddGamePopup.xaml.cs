using CommunityToolkit.Maui.Views;
using RikikiApp.Models;
using RikikiApp.Repositories;
using System.Diagnostics;
using RikikiApp.Services;

namespace RikikiApp.Views.Popups;

public partial class AddGamePopup : Popup
{
    public AddGamePopup(IGameRepository games)
    {
        InitializeComponent();
    }
}
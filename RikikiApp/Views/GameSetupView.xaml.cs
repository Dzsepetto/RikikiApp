using CommunityToolkit.Maui.Extensions;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Views.Popups;
using RikikiApp.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace RikikiApp.Views;

public partial class GameSetupView : ContentView
{
    private readonly NavigationService _nav;
    public GameSetupView(NavigationService nav)
    {
        InitializeComponent();
        _nav = nav;
    }
    private async void OnAddPlayerClicked(object sender, EventArgs e)
    {
        if (BindingContext is GameSetupVM vm)
            await vm.AddPlayer("valaki");
    }
    private void OnBackClicked(object sender, EventArgs e)
    {
        _nav.Pop();
    }
}
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.DependencyInjection;
using RikikiApp.Repositories;
using System.Diagnostics;

namespace RikikiApp.Features.Games.Views.Popups;

public partial class AddPlayerPopup : Popup
{
    public AddPlayerPopup()
    {
        InitializeComponent();

    }
    void TestClick(object sender, EventArgs e)
    {
        Debug.WriteLine("BUTTON CLICKED");
    }

}
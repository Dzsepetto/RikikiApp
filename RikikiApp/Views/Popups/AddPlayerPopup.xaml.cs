using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.DependencyInjection;
using RikikiApp.Models;
using RikikiApp.Repositories;
using System.Diagnostics;

namespace RikikiApp.Views.Popups;

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
using RikikiApp.ViewModel;
using System.Diagnostics;

namespace RikikiApp.Views;

public partial class GameSetupView : ContentView
{

    public GameSetupView(GameSetupVM vm)
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" XAML CRASH: {ex}");
            throw;
        }
    }
}
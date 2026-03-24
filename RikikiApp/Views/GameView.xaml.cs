using RikikiApp.ViewModel;
using System.Diagnostics;

namespace RikikiApp.Views;

public partial class GameView : ContentView
{
    private readonly GameViewVM _vm;

    public GameView(GameViewVM vm)
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"💥 XAML CRASH: {ex}");
            throw;
        }

        _vm = vm;
        BindingContext = _vm;
    }

}
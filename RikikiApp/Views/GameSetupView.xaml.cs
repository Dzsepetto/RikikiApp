using RikikiApp.Models;
using RikikiApp.ViewModels;
using System.Diagnostics;

namespace RikikiApp.Views;


public partial class GameSetupView : ContentView
{
    private GamePlayer _draggedItem;

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        var element = (BindableObject)sender;
        _draggedItem = element.BindingContext as GamePlayer;
    }

    private void OnDrop(object sender, DropEventArgs e)
    {
        var target = (sender as BindableObject)?.BindingContext as GamePlayer;

        if (_draggedItem == null || target == null)
            return;

        if (_draggedItem == target)
            return;

        var vm = BindingContext as GameSetupVM;

        vm?.MovePlayer(_draggedItem, target);

        e.Handled = true;
    }
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
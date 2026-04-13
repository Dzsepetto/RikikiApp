using RikikiApp.Models;
using RikikiApp.ViewModels.UiWrappers;
using System.Diagnostics;

namespace RikikiApp.Views;


public partial class GameSetupView : ContentView
{
    private GamePlayerItemVM _draggedItem;

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        var element = (BindableObject)sender;
        _draggedItem = element.BindingContext as GamePlayerItemVM;

        e.Data.Properties.Add("item", _draggedItem);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        var view = sender as View;
        var target = (view as BindableObject)?.BindingContext as GamePlayerItemVM;

        if (target == null)
            return;

        var vm = BindingContext as GameSetupVM;
        if (vm == null)
            return;

        foreach (var p in vm.Players)
            p.IsDropTarget = false;

        target.IsDropTarget = true;
    }
    private void OnDropToEnd(object sender, DropEventArgs e)
    {
        var vm = BindingContext as GameSetupVM;

        if (_draggedItem == null || vm == null)
            return;

        vm.MovePlayerToIndex(_draggedItem, vm.Players.Count);

        foreach (var p in vm.Players)
            p.IsDropTarget = false;

        _draggedItem = null;
    }

    private void OnDrop(object sender, DropEventArgs e)
    {
        var view = sender as View;
        var target = (view as BindableObject)?.BindingContext as GamePlayerItemVM;

        if (_draggedItem == null || target == null || view == null)
            return;

        var vm = BindingContext as GameSetupVM;
        if (vm == null)
            return;

        var position = e.GetPosition(view);

        if (position == null)
            return;

        bool isTopHalf = position.Value.Y < view.Height / 2;

        int fromIndex = vm.Players.IndexOf(_draggedItem);
        int toIndex = vm.Players.IndexOf(target);

        if (fromIndex < toIndex)
            toIndex--;

        if (!isTopHalf)
            toIndex++;

        vm.MovePlayerToIndex(_draggedItem, toIndex);

        foreach (var p in vm.Players)
            p.IsDropTarget = false;

        vm.IsDragging = false;
        _draggedItem = null;
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
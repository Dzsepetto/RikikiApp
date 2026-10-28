using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Features.Games.ViewModels.UiWrappers;
using RikikiApp.Features.Games.ViewModels;
using System.Diagnostics;

namespace RikikiApp.Features.Games.Views;


public partial class GameSetupView : ContentView
{
    private GamePlayerItemVM? _draggedItem;

    private void OnDragStarting(object sender,DragStartingEventArgs e)
    {
        var element = sender as BindableObject;

        _draggedItem =
            element?.BindingContext as GamePlayerItemVM;

        if (BindingContext is GameSetupVM vm)
            vm.IsDragging = _draggedItem != null;

        if (_draggedItem != null)
            e.Data.Properties["item"] = _draggedItem;
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

        var target = view?.BindingContext as GamePlayerItemVM;
        var vm = BindingContext as GameSetupVM;

        if (_draggedItem == null ||
            target == null ||
            view == null ||
            vm == null)
            return;

        try
        {
            var position = e.GetPosition(view);

            if (position == null)
                return;

            bool isTopHalf = position.Value.Y < view.Height / 2;

            int targetIndex = vm.Players.IndexOf(target);

            if (targetIndex < 0)
                return;

            int insertIndex = isTopHalf
                ? targetIndex
                : targetIndex + 1;

            vm.MovePlayerToIndex(_draggedItem, insertIndex);
        }
        finally
        {
            foreach (var p in vm.Players)
                p.IsDropTarget = false;

            vm.IsDragging = false;
            _draggedItem = null;
        }
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
using RikikiApp.Features.Games.ViewModels;
using RikikiApp.Features.Games.ViewModels.UiWrappers;
using System.Diagnostics;

namespace RikikiApp.Features.Games.Views;

public partial class GameSetupView : ContentView
{
    private GamePlayerItemVM? _draggedItem;
    private GamePlayerItemVM? _currentDropTarget;

    public GameSetupView(GameSetupVM vm)
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"XAML CRASH: {ex}");
            throw;
        }
    }

    // DRAG START
    private void OnDragStarting(
        object sender,
        DragStartingEventArgs e)
    {
        var element = sender as BindableObject;

        _draggedItem =
            element?.BindingContext as GamePlayerItemVM;

        if (BindingContext is not GameSetupVM vm)
            return;

        ClearDropTargets(vm);

        vm.IsDragging = _draggedItem != null;

        if (_draggedItem != null)
            e.Data.Properties["item"] = _draggedItem;
    }

    // DRAG OVER
    private void OnDragOver(
        object sender,
        DragEventArgs e)
    {
        if (_draggedItem == null)
            return;

        var view = sender as View;

        var target =
            view?.BindingContext as GamePlayerItemVM;

        if (target == null)
            return;

        if (BindingContext is not GameSetupVM vm)
            return;

        // Ha ugyanaz a célpont, nincs szükség UI-frissítésre.
        if (_currentDropTarget == target)
            return;

        ClearDropTargets(vm);

        target.IsDropTarget = true;
        _currentDropTarget = target;
    }

    // DROP ON PLAYER
    private void OnDrop(
        object sender,
        DropEventArgs e)
    {
        var view = sender as View;

        var target =
            view?.BindingContext as GamePlayerItemVM;

        var vm = BindingContext as GameSetupVM;

        if (vm == null)
            return;

        try
        {
            if (_draggedItem == null ||
                target == null ||
                view == null)
                return;

            if (_draggedItem == target)
                return;

            var position = e.GetPosition(view);

            if (position == null)
                return;

            bool isTopHalf =
                position.Value.Y < view.Height / 2;

            int targetIndex = vm.Players.IndexOf(target);

            if (targetIndex < 0)
                return;

            int insertIndex = isTopHalf
                ? targetIndex
                : targetIndex + 1;

            vm.MovePlayerToIndex(
                _draggedItem,
                insertIndex);
        }
        finally
        {
            ResetDragState(vm);
        }
    }

    // DROP AT END
    private void OnDropToEnd(
        object sender,
        DropEventArgs e)
    {
        var vm = BindingContext as GameSetupVM;

        if (vm == null)
            return;

        try
        {
            if (_draggedItem == null)
                return;

            vm.MovePlayerToIndex(
                _draggedItem,
                vm.Players.Count);
        }
        finally
        {
            ResetDragState(vm);
        }
    }

    // DRAG COMPLETED OR CANCELLED
    private void OnDragCompleted(
        object sender,
        DropCompletedEventArgs e)
    {
        if (BindingContext is GameSetupVM vm)
            ResetDragState(vm);
    }

    // CLEAR DROP INDICATORS
    private void ClearDropTargets(GameSetupVM vm)
    {
        if (_currentDropTarget != null)
        {
            _currentDropTarget.IsDropTarget = false;
            _currentDropTarget = null;
        }

        // Az előző állapotok biztos törlése.
        foreach (var player in vm.Players)
        {
            if (player.IsDropTarget)
                player.IsDropTarget = false;
        }
    }

    // RESET DRAG STATE
    private void ResetDragState(GameSetupVM vm)
    {
        ClearDropTargets(vm);

        vm.IsDragging = false;
        _draggedItem = null;
    }
}
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.DependencyInjection;
using RikikiApp.Core.Abstractions;
using RikikiApp.Core.Popups;
using RikikiApp.Features.Main.Views;

public class NavigationService
{
    private readonly IServiceProvider _services;

    private readonly Stack<(View view, object vm)> _stack = new();

    private readonly SemaphoreSlim _navigationLock = new(1, 1);

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    private MainLayoutPage GetMainLayout()
    {
        return Application.Current?
            .Windows.FirstOrDefault()?.Page as MainLayoutPage
            ?? throw new InvalidOperationException(
                "MainLayoutPage not found.");
    }

    private async Task InitializeAsync<TViewModel>(
        TViewModel vm,
        Func<TViewModel, Task>? initAsync)
        where TViewModel : class
    {
        if (initAsync != null)
        {
            await initAsync(vm);
        }
        else if (vm is IInitializable initializable)
        {
            await initializable.InitAsync();
        }
    }

    // COMMON NAVIGATION GUARD
    private async Task ExecuteNavigationAsync(
        Func<Task> action,
        bool showLoading = false)
    {
        // Ignore repeated navigation taps
        if (!await _navigationLock.WaitAsync(0))
            return;

        try
        {
            if (showLoading)
            {
                await RunWithLoading(action);
            }
            else
            {
                await action();
            }
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    // INTERNAL ROOT
    private async Task SetRootCoreAsync<TView, TViewModel>(
        Func<TViewModel, Task>? initAsync)
        where TView : View
        where TViewModel : class
    {
        var view = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        view.BindingContext = vm;

        await InitializeAsync(vm, initAsync);

        var layout = GetMainLayout();

        layout.SetContent(view);

        _stack.Clear();
        _stack.Push((view, vm));
    }

    // INTERNAL PUSH
    private async Task PushCoreAsync<TView, TViewModel>(
        Func<TViewModel, Task>? initAsync)
        where TView : View
        where TViewModel : class
    {
        var view = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        view.BindingContext = vm;

        await InitializeAsync(vm, initAsync);

        GetMainLayout().SetContent(view);

        _stack.Push((view, vm));
    }

    // SET ROOT
    public Task SetRoot<TView, TViewModel>(
        Func<TViewModel, Task>? initAsync = null)
        where TView : View
        where TViewModel : class
    {
        return ExecuteNavigationAsync(
            () => SetRootCoreAsync<TView, TViewModel>(initAsync));
    }

    // SET ROOT WITH LOADING
    public Task SetRootWithLoading<TView, TViewModel>(
        Func<TViewModel, Task>? initAsync = null)
        where TView : View
        where TViewModel : class
    {
        return ExecuteNavigationAsync(
            () => SetRootCoreAsync<TView, TViewModel>(initAsync),
            showLoading: true);
    }

    // PUSH
    public Task Push<TView, TViewModel>(
        Func<TViewModel, Task>? initAsync = null)
        where TView : View
        where TViewModel : class
    {
        return ExecuteNavigationAsync(
            () => PushCoreAsync<TView, TViewModel>(initAsync));
    }

    // PUSH WITH LOADING
    public Task PushWithLoading<TView, TViewModel>(
        Func<TViewModel, Task>? initAsync = null)
        where TView : View
        where TViewModel : class
    {
        return ExecuteNavigationAsync(
            () => PushCoreAsync<TView, TViewModel>(initAsync),
            showLoading: true);
    }

    // POP
    public Task Pop()
    {
        return ExecuteNavigationAsync(async () =>
        {
            if (_stack.Count <= 1)
                return;

            var previous = _stack.ElementAt(1);

            if (previous.vm is IRefreshable refreshable)
                await refreshable.RefreshAsync();

            GetMainLayout().SetContent(previous.view);

            _stack.Pop();
        });
    }

    // NORMAL POPUPS
    public async Task<TResult?> ShowPopupAsync<TView, TViewModel, TResult>(
      Func<TViewModel, Task>? initAsync = null,
      bool showLoading = false)
      where TView : Popup
      where TViewModel : class
    {
        var layout = GetMainLayout();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        bool loadingVisible = false;

        void HideLoading()
        {
            if (!loadingVisible)
                return;

            loadingVisible = false;
            layout.SetLoading(false);
        }

        void OnPopupOpened(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[Popup] Opened: {stopwatch.ElapsedMilliseconds} ms");

            HideLoading();
        }

        TView? popup = null;

        try
        {
            if (showLoading)
            {
                layout.SetLoading(true);
                loadingVisible = true;

                await Task.Yield();
            }

            popup = _services.GetRequiredService<TView>();

            var vm = _services.GetRequiredService<TViewModel>();

            popup.BindingContext = vm;

            if (vm is IPopupAware popupVm)
                popupVm.PopupInstance = popup;

            await InitializeAsync(vm, initAsync);

            System.Diagnostics.Debug.WriteLine(
                $"[Popup] Initialized: {stopwatch.ElapsedMilliseconds} ms");

            popup.CanBeDismissedByTappingOutsideOfPopup = false;

            popup.Opened += OnPopupOpened;

            var resultTask =
                (vm as IPopupResults<TResult>)?.ResultTask;

            await layout.ShowPopupAsync(popup);

            System.Diagnostics.Debug.WriteLine(
                $"[Popup] Closed: {stopwatch.ElapsedMilliseconds} ms");

            if (resultTask?.IsCompletedSuccessfully == true)
                return await resultTask;

            return default;
        }
        finally
        {
            if (popup != null)
                popup.Opened -= OnPopupOpened;

            HideLoading();
        }
    }

    // LOADING OVERLAY
    public async Task RunWithLoading(Func<Task> action)
    {
        var layout = GetMainLayout();

        try
        {
            layout.SetLoading(true);

            await Task.Yield();

            await action();
        }
        finally
        {
            layout.SetLoading(false);
        }
    }
}
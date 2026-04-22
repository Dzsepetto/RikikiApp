using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using RikikiApp.Core.Abstractions;
using RikikiApp.Core.Popups;
using RikikiApp.Features.Main.Views;
using RikikiApp.Features.PopupsGeneral.Views;

public class NavigationService
{
    private readonly IServiceProvider _services;
    private readonly Stack<View> _stack = new();

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    private MainLayoutPage GetMainLayout()
        => (MainLayoutPage)Application.Current.MainPage;

    public async Task SetRoot<TView, TViewModel>(Func<TViewModel, Task>? initAsync = null)
        where TView : View
        where TViewModel : class
    {
        var view = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        view.BindingContext = vm;

        if (initAsync != null)
        {
            await initAsync(vm);
        }
        else if (vm is IInitializable initVm)
        {
            await initVm.InitAsync();
        }

        _stack.Clear();
        _stack.Push(view);

        GetMainLayout().SetContent(view);
    }
    public async Task SetRootWithLoading<TView, TViewModel>(Func<TViewModel, Task>? initAsync = null)
    where TView : View
    where TViewModel : class
    {
        await RunWithLoading(async () =>
        {
            var view = _services.GetRequiredService<TView>();
            var vm = _services.GetRequiredService<TViewModel>();

            view.BindingContext = vm;

            if (initAsync != null)
            {
                await initAsync(vm);
            }
            else if (vm is IInitializable initVm)
            {
                await initVm.InitAsync();
            }

            _stack.Clear();
            _stack.Push(view);

            GetMainLayout().SetContent(view);
        });
    }

    public async Task PushWithLoading<TView, TViewModel>(Func<TViewModel, Task>? initAsync = null)
        where TView : View
        where TViewModel : class
    {
        await RunWithLoading(async () =>
        {
            await Push<TView, TViewModel>(initAsync);
        });
    }

    public async Task Push<TView, TViewModel>(Func<TViewModel, Task>? initAsync = null)
        where TView : View
        where TViewModel : class
    {
        var view = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        view.BindingContext = vm;

        if (initAsync != null)
        {
            await initAsync(vm);
        }
        else if (vm is IInitializable initVm)
        {
            await initVm.InitAsync();
        }

        _stack.Push(view);
        GetMainLayout().SetContent(view);
    }

    public Task Pop()
    {
        if (_stack.Count <= 1)
            return Task.CompletedTask;

        _stack.Pop();

        var view = _stack.Peek();
        GetMainLayout().SetContent(view);

        return Task.CompletedTask;
    }

    public async Task<TResult?> ShowPopupAsync<TView, TViewModel, TResult>(
        Func<TViewModel, Task>? initAsync = null)
        where TView : Popup
        where TViewModel : class
    {
        var popup = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        popup.BindingContext = vm;

        if (vm is IPopupAware popupVm)
            popupVm.PopupInstance = popup;

        if (initAsync != null)
        {
            await initAsync(vm);
        }
        else if (vm is IInitializable initVm)
        {
            await initVm.InitAsync();
        }

        var page = Application.Current!.MainPage!;
        await page.ShowPopupAsync(popup);

        if (vm is IPopupResults<TResult> resultVm)
            return await resultVm.ResultTask;

        return default;
    }
    public async Task<TResult?> ShowPopupWithLoadingAsync<TView, TViewModel, TResult>(
    Func<TViewModel, Task>? initAsync = null)
    where TView : Popup
    where TViewModel : class
    {
        var popup = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        popup.BindingContext = vm;

        if (vm is IPopupAware popupVm)
            popupVm.PopupInstance = popup;

        await RunWithLoading(async () =>
        {
            if (initAsync != null)
            {
                await initAsync(vm);
            }
            else if (vm is IInitializable initVm)
            {
                await initVm.InitAsync();
            }
        });

        var page = Application.Current!.MainPage!;
        await page.ShowPopupAsync(popup);

        if (vm is IPopupResults<TResult> resultVm)
            return await resultVm.ResultTask;

        return default;
    }
    public async Task RunWithLoading(Func<Task> action)
    {
        var popup = _services.GetRequiredService<LoadingPopup>();
        var page = Application.Current.MainPage!;

        popup.CanBeDismissedByTappingOutsideOfPopup = false;

        _ = page.ShowPopupAsync(popup);

        await Task.Delay(50);

        try
        {
            await action();
        }
        finally
        {
            await popup.CloseAsync();
        }
    }
}
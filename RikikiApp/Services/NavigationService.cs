using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using RikikiApp.Services;
using RikikiApp.ViewModel.Popups;
using RikikiApp.Views;
using RikikiApp.Views.Popups;
using static SQLite.SQLite3;

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

    // 🔹 ROOT (tab váltásnál)
    public void SetRoot<TView>() where TView : View
    {
        var view = _services.GetRequiredService<TView>();

        _stack.Clear();
        _stack.Push(view);

        GetMainLayout().SetContent(view);
    }
    public async Task SetRoot<TView, TViewModel>(Action<TViewModel>? init = null)
    where TView : View
    where TViewModel : class
    {
        var view = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        init?.Invoke(vm);
        view.BindingContext = vm;

        _stack.Clear();
        _stack.Push(view);

        GetMainLayout().SetContent(view);

        if (vm is IInitializable initVm)
            await initVm.InitAsync();
    }

    // 🔹 NAVIGATE FORWARD
    public void Push<TView>() where TView : View
    {
        var view = _services.GetRequiredService<TView>();

        _stack.Push(view);
        GetMainLayout().SetContent(view);
    }

    public async Task Push<TView, TViewModel>(Action<TViewModel>? init = null)
        where TView : View
        where TViewModel : class
    {
        var view = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        init?.Invoke(vm);
        view.BindingContext = vm;

        _stack.Push(view);
        GetMainLayout().SetContent(view);

        if (vm is IInitializable initVm)
            await initVm.InitAsync();
    }

    // 🔹 BACK
    public async Task Pop()
    {
        if (_stack.Count <= 1)
            return;

        _stack.Pop();

        var view = _stack.Peek();
        GetMainLayout().SetContent(view);

        if (view.BindingContext is IInitializable initVm)
            await initVm.InitAsync();
    }

    // 🔹 POPUP 
    public async Task<TResult?> ShowPopupAsync<TView, TViewModel, TResult>(
        Action<TViewModel>? init = null)
        where TView : Popup
        where TViewModel : class
    {
        var popup = _services.GetRequiredService<TView>();
        var vm = _services.GetRequiredService<TViewModel>();

        init?.Invoke(vm);

        popup.BindingContext = vm;

        if (vm is IPopupAware popupVm)
            popupVm.PopupInstance = popup;

        if (vm is IInitializable initVm)
            await initVm.InitAsync();

        var page = Application.Current!.MainPage!;
        await page.ShowPopupAsync(popup);

        if (vm is IPopupResults<TResult> resultVm)
            return await resultVm.ResultTask;

        return default;
    }
}
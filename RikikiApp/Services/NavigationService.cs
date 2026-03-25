using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using RikikiApp.Services;
using RikikiApp.Views;
using RikikiApp.Views.Popups;

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
    public async Task<T?> ShowPopupAsync<T>(Popup popup) where T : class
    {
        var page = Application.Current!.MainPage!;
        await page.ShowPopupAsync(popup);

        if (popup is IPopupResult<T> resultPopup)
            return await resultPopup.ResultTask;

        return null;
    }
}
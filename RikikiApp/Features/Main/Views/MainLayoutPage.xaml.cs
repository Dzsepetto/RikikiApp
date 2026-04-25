using RikikiApp.Features.Main.ViewModels;

namespace RikikiApp.Features.Main.Views;

public partial class MainLayoutPage : ContentPage
{
    private readonly NavigationService _nav;
    private bool _initialized;

    public MainLayoutPage(NavigationService nav)
    {
        InitializeComponent();
        _nav = nav;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is MainLayoutVM vm)
        {
            vm.OnTabChanged = async (tab) => await UpdateTabUI(tab);
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
            return;

        _initialized = true;

        _nav.SetRoot<MainView, MainViewVM>();

        if (BindingContext is MainLayoutVM vm)
            _ = UpdateTabUI(vm.CurrentTab);
    }

    public void SetContent(View view)
    {
        PageContent.Content = view;
    }

    async Task SetSelected(View view, bool selected)
    {
        if (selected)
        {
            await Task.WhenAll(
                view.TranslateTo(0, -12, 120, Easing.CubicOut),
                view.ScaleTo(1.1, 120, Easing.CubicOut)
            );
        }
        else
        {
            await Task.WhenAll(
                view.TranslateTo(0, 0, 120, Easing.CubicOut),
                view.ScaleTo(1.0, 120, Easing.CubicOut)
            );
        }
    }

    async Task UpdateTabUI(MainLayoutVM.TabType tab)
    {
        await Task.WhenAll(
            SetSelected(HomeButton, tab == MainLayoutVM.TabType.Home),
            SetSelected(ProfileButton, tab == MainLayoutVM.TabType.Profile),
            SetSelected(StatsButton, tab == MainLayoutVM.TabType.Stats)
        );
    }
}
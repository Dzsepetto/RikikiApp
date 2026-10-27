using RikikiApp.Features.Main.ViewModels;
using System.Diagnostics;

namespace RikikiApp.Features.Main.Views;

public partial class MainLayoutPage : ContentPage
{
    private readonly NavigationService _nav;

    private bool _initialized;
    private bool _isLoading;
    private bool _loadingAnimationRunning;

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
            vm.OnTabChanged = async (tab) =>
                await UpdateTabUI(tab);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
            return;

        _initialized = true;

        try
        {
            await _nav.SetRoot<MainView, MainViewVM>();

            if (BindingContext is MainLayoutVM vm)
                await UpdateTabUI(vm.CurrentTab);
        }
        catch (Exception ex)
        {
            _initialized = false;
            Debug.WriteLine(
                $"MainLayout initialization error: {ex}");
        }
    }

    public void SetContent(View view)
    {
        PageContent.Content = view;
    }

    // LOADING OVERLAY
    public void SetLoading(bool isLoading)
    {
        _isLoading = isLoading;

        LoadingOverlay.IsVisible = isLoading;

        if (isLoading)
        {
            _ = AnimateLoadingAsync();
        }
        else
        {
            PokerChipImage.CancelAnimations();
            PokerChipImage.Rotation = 0;
        }
    }

    // ROTATING POKER CHIP
    private async Task AnimateLoadingAsync()
    {
        if (_loadingAnimationRunning)
            return;

        _loadingAnimationRunning = true;

        try
        {
            while (_isLoading)
            {
                PokerChipImage.Rotation = 0;

                await PokerChipImage.RotateToAsync(
                    360,
                    900,
                    Easing.Linear
                );
            }
        }
        finally
        {
            _loadingAnimationRunning = false;

            if (!_isLoading)
                PokerChipImage.Rotation = 0;
        }
    }

    async Task SetSelected(View view, bool selected)
    {
        if (selected)
        {
            await Task.WhenAll(
                view.TranslateToAsync(
                    0, -12, 120, Easing.CubicOut),

                view.ScaleToAsync(
                    1.1, 120, Easing.CubicOut)
            );
        }
        else
        {
            await Task.WhenAll(
                view.TranslateToAsync(
                    0, 0, 120, Easing.CubicOut),

                view.ScaleToAsync(
                    1.0, 120, Easing.CubicOut)
            );
        }
    }

    async Task UpdateTabUI(MainLayoutVM.TabType tab)
    {
        await Task.WhenAll(
            SetSelected(
                HomeButton,
                tab == MainLayoutVM.TabType.Home),

            SetSelected(
                ProfileButton,
                tab == MainLayoutVM.TabType.Profile),

            SetSelected(
                StatsButton,
                tab == MainLayoutVM.TabType.Stats)
        );
    }
}
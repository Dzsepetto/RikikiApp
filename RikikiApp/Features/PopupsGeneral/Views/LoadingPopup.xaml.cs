namespace RikikiApp.Features.PopupsGeneral.Views;

using CommunityToolkit.Maui.Views;

public partial class LoadingPopup : Popup
{
    private bool _isAnimating;

    public LoadingPopup()
    {
        InitializeComponent();

        this.Opened += OnOpened;
        this.Closed += OnClosed;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        _isAnimating = true;

        while (_isAnimating)
        {
            PokerChipImage.Rotation = 0;
            await PokerChipImage.RotateToAsync(360, 900, Easing.Linear);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _isAnimating = false;
    }
}
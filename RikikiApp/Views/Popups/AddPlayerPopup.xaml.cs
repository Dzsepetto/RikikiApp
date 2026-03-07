using CommunityToolkit.Maui.Views;

namespace RikikiApp.Views.Popups;

public partial class AddPlayerPopup : Popup
{
    public string? Result { get; private set; }

    public AddPlayerPopup()
    {
        InitializeComponent();
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        Result = null;
        await CloseAsync();
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                "Name required",
                "OK");
            return;
        }

        Result = NameEntry.Text;

        await CloseAsync();
    }
}
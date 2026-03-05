using CommunityToolkit.Maui.Views;

namespace RikikiApp.Views.Popups;

public partial class AddPlayerPopup : Popup
{
    public AddPlayerPopup Result { get; private set; }
    public string GuestName { get; private set; } = "";
    public int Seat { get; private set; }

    public AddPlayerPopup(int defaultSeat)
    {
        InitializeComponent();
        SeatEntry.Text = defaultSeat.ToString();
    }

    void OnPlusClicked(object sender, EventArgs e)
    {
        if (int.TryParse(SeatEntry.Text, out int seat))
            SeatEntry.Text = (seat + 1).ToString();
    }

    void OnMinusClicked(object sender, EventArgs e)
    {
        if (int.TryParse(SeatEntry.Text, out int seat) && seat > 1)
            SeatEntry.Text = (seat - 1).ToString();
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

        GuestName = NameEntry.Text;

        if (int.TryParse(SeatEntry.Text, out int seat))
            Seat = seat;
        else
            Seat = 1;

        Result = this;

        await CloseAsync();
    }
}
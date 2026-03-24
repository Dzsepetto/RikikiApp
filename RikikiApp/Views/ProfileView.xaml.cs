using RikikiApp.ViewModels;

namespace RikikiApp.Views;

public partial class ProfileView : ContentView
{
    private readonly ProfileViewVM _vm;
    public ProfileView(ProfileViewVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    private async void OnLoaded(object sender, EventArgs e)
    {
        await _vm.Initialize();
    }
}
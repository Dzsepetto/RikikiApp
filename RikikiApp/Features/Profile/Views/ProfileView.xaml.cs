using System.ComponentModel;
using RikikiApp.ViewModels;

namespace RikikiApp.Features.Profile.Views;

public partial class ProfileView : ContentView
{
    private ProfileViewVM? _vm;

    public ProfileView()
    {
        InitializeComponent();
        BindingContextChanged += ProfileView_BindingContextChanged;
    }

    private void ProfileView_BindingContextChanged(object? sender, EventArgs e)
    {
        if (_vm != null)
            _vm.PropertyChanged -= Vm_PropertyChanged;

        _vm = BindingContext as ProfileViewVM;

        if (_vm != null)
            _vm.PropertyChanged += Vm_PropertyChanged;
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProfileViewVM vm)
            return;

        if (e.PropertyName == nameof(ProfileViewVM.IsEditingName))
        {
            Dispatcher.Dispatch(async () =>
            {
                if (vm.IsEditingName)
                {
                    await Task.Delay(150);
                    UserNameEntry.Focus();
                }
                else
                {
                    UserNameEntry.Unfocus();
                }
            });
        }
    }
}
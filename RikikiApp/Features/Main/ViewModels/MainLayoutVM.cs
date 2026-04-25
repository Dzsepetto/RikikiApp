using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Features.Main.Views;
using RikikiApp.Features.Profile.ViewModels;
using RikikiApp.Features.Profile.Views;
using RikikiApp.Features.Stats.ViewModels;
using RikikiApp.Features.Stats.Views;

namespace RikikiApp.Features.Main.ViewModels
{
    public partial class MainLayoutVM : ObservableObject
    {
        private readonly NavigationService _nav;

        [ObservableProperty]
        private TabType currentTab;

        public Action<TabType>? OnTabChanged;

        public MainLayoutVM(NavigationService nav)
        {
            _nav = nav;
            currentTab = TabType.Home;
        }

        [RelayCommand]
        private async Task GoHome()
        {
            CurrentTab = TabType.Home;
            OnTabChanged?.Invoke(CurrentTab);
            await _nav.SetRoot<MainView, MainViewVM>();
        }

        [RelayCommand]
        private async Task GoProfile()
        {
            CurrentTab = TabType.Profile;
            OnTabChanged?.Invoke(CurrentTab);
            await _nav.SetRoot<ProfileView, ProfileViewVM>(async vm => await vm.InitAsync());
        }

        [RelayCommand]
        private async Task GoStats()
        {
            CurrentTab = TabType.Stats;
            OnTabChanged?.Invoke(CurrentTab);
            await _nav.SetRoot<StatsView, StatsViewVM>();
        }

        public enum TabType
        {
            Home,
            Profile,
            Stats
        }
    }
}

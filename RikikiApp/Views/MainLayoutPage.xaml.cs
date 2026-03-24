using System.Data;

namespace RikikiApp.Views;

public partial class MainLayoutPage : ContentPage
{
    private TabType _currentTab = TabType.Home;

    private readonly NavigationService _nav;
    private bool _initialized;
    public MainLayoutPage(NavigationService nav)
    {
        InitializeComponent();

        _nav = nav;

        UpdateTabUI();
    }
    public void SetContent(View view)
    {
        PageContent.Content = view;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
            return;

        _initialized = true;

        _nav.SetRoot<MainView>();
    }
    void GoHome(object sender, EventArgs e)
    {
        _currentTab = TabType.Home;
        _nav.SetRoot<MainView>();
        UpdateTabUI();
    }

    void GoProfile(object sender, EventArgs e)
    {
        _currentTab = TabType.Profile;
        _nav.SetRoot<ProfileView>();
        UpdateTabUI();
    }

    void GoStats(object sender, EventArgs e)
    {
        _currentTab = TabType.Stats;
        _nav.SetRoot<StatsView>();
        UpdateTabUI();
    }
    void UpdateTabUI()
    {
        // reset
        HomeButton.TranslationY = 0;
        ProfileButton.TranslationY = 0;
        StatsButton.TranslationY = 0;

        // aktív feljebb
        switch (_currentTab)
        {
            case TabType.Home:
                HomeButton.TranslationY = -10;
                break;

            case TabType.Profile:
                ProfileButton.TranslationY = -10;
                break;

            case TabType.Stats:
                StatsButton.TranslationY = -10;
                break;
        }
    }
    private enum TabType
    {
        Home,
        Profile,
        Stats
    }
}


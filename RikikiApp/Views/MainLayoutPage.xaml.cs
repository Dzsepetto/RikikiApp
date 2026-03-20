using System.Data;

namespace RikikiApp.Views;

public partial class MainLayoutPage : ContentPage
{
    private TabType _currentTab = TabType.Home;

    private readonly MainPage _mainPage = new();
    private readonly ProfilePage _profilePage = new();
    private readonly StatsPage _statsPage = new();

    public MainLayoutPage()
	{
		InitializeComponent();
        UpdateTabUI();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        PageContent.Content = _mainPage.Content;

    }
    void GoHome(object sender, EventArgs e)
    {
        _currentTab = TabType.Home;
        PageContent.Content = _mainPage.Content;
        UpdateTabUI();
    }

    void GoProfile(object sender, EventArgs e)
    {
        _currentTab = TabType.Profile;
        PageContent.Content = _profilePage.Content;
        UpdateTabUI();
    }

    void GoStats(object sender, EventArgs e)
    {
        _currentTab = TabType.Stats;
        PageContent.Content = _statsPage.Content;
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


using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RikikiApp.ViewModels;

public partial class CallViewVM : ObservableObject
{
    [ObservableProperty]
    private int called;

    [ObservableProperty]
    private int won;

    [ObservableProperty]
    private int maxValue;

    [ObservableProperty]
    private bool isCallEnabled;

    [ObservableProperty]
    private bool isWonEnabled;

    [ObservableProperty]
    private bool isWonVisible;

    public int CallId { get; set; }
    public int GamePlayerId { get; set; }
    public string PlayerName { get; set; } = "";

    [RelayCommand]
    private void IncreaseCall()
    {
        if (!IsCallEnabled)
            return;

        if (Called < MaxValue)
            Called++;
    }

    [RelayCommand]
    private void DecreaseCall()
    {
        if (!IsCallEnabled)
            return;

        if (Called > 0)
            Called--;
    }

    [RelayCommand]
    private void IncreaseWon()
    {
        if (!IsWonEnabled)
            return;

        if (Won < MaxValue)
            Won++;
    }

    [RelayCommand]
    private void DecreaseWon()
    {
        if (!IsWonEnabled)
            return;

        if (Won > 0)
            Won--;
    }
}
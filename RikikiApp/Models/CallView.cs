using System.ComponentModel;
using System.Runtime.CompilerServices;

public class CallView : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private double _called;
    public double Called
    {
        get => _called;
        set
        {
            if (_called != value)
            {
                _called = value;
                OnPropertyChanged();
            }
        }
    }

    private double _won;
    public double Won
    {
        get => _won;
        set
        {
            if (_won != value)
            {
                _won = value;
                OnPropertyChanged();
            }
        }
    }

    public double MaxValue { get; set; }

    public int CallId { get; set; }
    public int GamePlayerId { get; set; }
    public string PlayerName { get; set; } = "";

    public bool IsCallEnabled { get; set; }
    public bool IsWonEnabled { get; set; }
    public bool IsWonVisible { get; set; }

    private void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
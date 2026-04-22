using System.Windows.Input;

namespace RikikiApp.Features.General.Views;

public partial class FancyMenuButton : ContentView
{
    public FancyMenuButton()
    {
        InitializeComponent();
    }

    public event EventHandler? Clicked;

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(FancyMenuButton), string.Empty);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(FancyMenuButton), default(ImageSource));

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FancyMenuButton));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(FancyMenuButton));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        await this.ScaleTo(0.96, 70);
        await this.ScaleTo(1.0, 70);

        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);

        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
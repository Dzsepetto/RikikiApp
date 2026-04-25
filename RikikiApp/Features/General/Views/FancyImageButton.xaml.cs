using System.Windows.Input;

namespace RikikiApp.Features.General.Views;

public partial class FancyImageButton : ContentView
{
	public FancyImageButton()
	{
		InitializeComponent();
	}
    bool _pressed;

    // 🔹 IMAGE SOURCE
    public static readonly BindableProperty SourceProperty =
        BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(FancyImageButton));

    public ImageSource Source
    {
        get => (ImageSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    // 🔹 ASPECT
    public static readonly BindableProperty AspectProperty =
        BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(FancyImageButton), Aspect.AspectFill);

    public Aspect Aspect
    {
        get => (Aspect)GetValue(AspectProperty);
        set => SetValue(AspectProperty, value);
    }

    // 🔹 COMMAND
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FancyImageButton));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    // 🔹 PARAM
    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(FancyImageButton));

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    // 🔥 PRESS ANIMATION
    private async void OnPressed(object sender, PointerEventArgs e)
    {
        if (_pressed) return;
        _pressed = true;

        await BtnImage.ScaleTo(0.9, 80, Easing.CubicOut);
    }

    private async void OnReleased(object sender, PointerEventArgs e)
    {
        if (!_pressed) return;

        await BtnImage.ScaleTo(1.0, 80, Easing.CubicOut);
        _pressed = false;
    }
}
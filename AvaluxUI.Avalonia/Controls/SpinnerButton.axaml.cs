using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaluxUI.Controls;

public partial class SpinnerButton : Button
{
    protected override Type StyleKeyOverride => typeof(Button);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SpinnerButton, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<Func<Task>?> ActionProperty =
        AvaloniaProperty.Register<SpinnerButton, Func<Task>?>(nameof(Action));

    public Func<Task>? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    public SpinnerButton()
    {
        InitializeComponent();
        PropertyChanged += (sender, args) =>
        {
            if (args.Property == TextProperty)
                TextBlock.Text = Text;
        };
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Action == null) return;
        SpinnerPanel.IsVisible = true;
        SpinnerPanel.Width = TextBlock.DesiredSize.Width;
        SpinnerPanel.Height = TextBlock.DesiredSize.Height;
        var w = double.Min(TextBlock.DesiredSize.Width, TextBlock.DesiredSize.Height);
        Spinner.Width = w;
        Spinner.Height = w;
        TextBlock.IsVisible = false;

        try
        {
            await Action();
        }
        catch (TaskCanceledException)
        {
        }

        SpinnerPanel.IsVisible = false;
        TextBlock.IsVisible = true;
    }
}
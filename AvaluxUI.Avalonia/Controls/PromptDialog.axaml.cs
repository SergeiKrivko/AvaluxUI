using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AvaluxUI.Controls;

public partial class PromptDialog : Window
{
    protected PromptDialog(string prompt)
    {
        InitializeComponent();
        TextBlock.Text = prompt;
    }

    protected void AddButton(Button button)
    {
        ButtonsPanel.Children.Add(button);
    }

    public static Task<bool> Prompt(string prompt)
    {
        return PromptDialog<bool>.Prompt(prompt,
        [
            new PromptDialogButton<bool>("Нет", false),
            new PromptDialogButton<bool>("Да", true, ButtonAppearance.Accent)
        ]);
    }

    public static Task<T?> Prompt<T>(string prompt, IEnumerable<PromptDialogButton<T>> buttons)
    {
        return PromptDialog<T>.Prompt(prompt, buttons);
    }
}

internal sealed class PromptDialog<T> : PromptDialog
{
    private T? _result = default;

    private PromptDialog(string prompt, IEnumerable<PromptDialogButton<T>> buttons) : base(prompt)
    {
        foreach (var button in buttons)
        {
            Button btn;
            if (button.OnConfirm != null)
            {
                btn = new SpinnerButton
                {
                    Content = button.Text, Action = async () =>
                    {
                        await button.OnConfirm();
                        _result = button.ReturnValue;
                        Close();
                    }
                };
            }
            else
            {
                btn = new Button { Content = button.Text };
                btn.Click += (sender, args) =>
                {
                    _result = button.ReturnValue;
                    Close();
                };
            }

            btn.Classes.AddRange(GetClasses(button.Appearance));

            AddButton(btn);
        }
    }

    private static Classes GetClasses(ButtonAppearance appearance)
    {
        return appearance switch
        {
            ButtonAppearance.Default => [],
            ButtonAppearance.Border => ["Border"],
            ButtonAppearance.Accent => ["Accent"],
            ButtonAppearance.Success => ["Success"],
            ButtonAppearance.Warning => ["Warning"],
            ButtonAppearance.Danger => ["Danger"],
            _ => [],
        };
    }

    public static async Task<T?> Prompt(string prompt, IEnumerable<PromptDialogButton<T>> buttons,
        Window? owner = null)
    {
        if (owner == null)
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
            {
                owner = desktop.MainWindow;
            }
            else
                return default;

        var dialog = new PromptDialog<T>(prompt, buttons);
        await dialog.ShowDialog(owner);
        return dialog._result;
    }
}
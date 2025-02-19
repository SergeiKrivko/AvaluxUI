using System;
using System.Threading.Tasks;

namespace AvaluxUI.Controls;

public record PromptDialogButton<T>(
    string Text,
    T ReturnValue,
    ButtonAppearance Appearance = ButtonAppearance.Default,
    Func<Task>? OnConfirm = null);
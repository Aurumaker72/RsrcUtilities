using RsrcArchitect.ViewModels.Controls;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Factories;

internal static class ControlViewModelFactory
{
    /// <summary>
    ///     Creates a class inheriting <see cref="ControlViewModel" /> which maps to the specified control
    /// </summary>
    /// <returns>An instance of a class inheriting <see cref="ControlViewModel" /></returns>
    internal static ControlViewModel Create(Control control, Func<string, bool> isIdentifierInUse)
    {
        return control switch
        {
            Button button => new ButtonViewModel(button, isIdentifierInUse),
            CheckBox checkBox => new CheckBoxViewModel(checkBox, isIdentifierInUse),
            GroupBox groupBox => new GroupBoxViewModel(groupBox, isIdentifierInUse),
            TextBox textBox => new TextBoxViewModel(textBox, isIdentifierInUse),
            _ => throw new ArgumentException($"{control} doesn't map to any ViewModel")
        };
    }
}
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Controls;

public class TextBoxViewModel : ControlViewModel
{
    public TextBoxViewModel(TextBox textBox, Func<string, bool> isIdentifierInUse) : base(textBox, isIdentifierInUse)
    {
    }

    private TextBox TextBox => (TextBox)Control;

    public bool IsWritable
    {
        get => TextBox.IsWriteable;
        set
        {
            TextBox.IsWriteable = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    public bool AllowHorizontalScroll
    {
        get => TextBox.AllowHorizontalScroll;
        set
        {
            TextBox.AllowHorizontalScroll = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
}
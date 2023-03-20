using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using RsrcUtilities.Controls;
using RsrcUtilities.RsrcArchitect.ViewModels.Messages;

namespace RsrcUtilities.RsrcArchitect.ViewModels.Controls;

public class TextBoxViewModel : ControlViewModel
{
    private TextBox TextBox => (TextBox)Control;
    
    public TextBoxViewModel(TextBox textBox, Func<string, bool> isIdentifierInUse) : base(textBox, isIdentifierInUse)
    {
    }
    
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
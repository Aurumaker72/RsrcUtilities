using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using RsrcUtilities.Controls;
using RsrcUtilities.RsrcArchitect.ViewModels.Messages;

namespace RsrcUtilities.RsrcArchitect.ViewModels.Controls;

public class ButtonViewModel : ControlViewModel
{
    private Button Button => (Button)Control;
    
    public ButtonViewModel(Button button, Func<string, bool> isIdentifierInUse) : base(button, isIdentifierInUse)
    {
    }
    
    public string Caption
    {
        get => Button.Caption;
        set
        {
            Button.Caption = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

}
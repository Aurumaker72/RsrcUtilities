using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Controls;

public class ButtonViewModel : ControlViewModel
{
    public ButtonViewModel(Button button, Func<string, bool> isIdentifierInUse) : base(button, isIdentifierInUse)
    {
    }

    private Button Button => (Button)Control;

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
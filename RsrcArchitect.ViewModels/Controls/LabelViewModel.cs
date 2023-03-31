using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Controls;

public class LabelViewModel : ControlViewModel
{
    public LabelViewModel(Label label, Func<string, bool> isIdentifierInUse) : base(label, isIdentifierInUse)
    {
    }
    
    private Label Label => (Label)Control;

    public string Caption
    {
        get => Label.Caption;
        set
        {
            Label.Caption = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
}
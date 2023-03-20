using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Controls;

public class GroupBoxViewModel : ControlViewModel
{
    public GroupBoxViewModel(GroupBox groupBox, Func<string, bool> isIdentifierInUse) : base(groupBox,
        isIdentifierInUse)
    {
    }

    private GroupBox GroupBox => (GroupBox)Control;

    public string Caption
    {
        get => GroupBox.Caption;
        set
        {
            GroupBox.Caption = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
}
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using RsrcUtilities.Controls;
using RsrcUtilities.RsrcArchitect.ViewModels.Messages;

namespace RsrcUtilities.RsrcArchitect.ViewModels.Controls;

public class GroupBoxViewModel : ControlViewModel
{
    private GroupBox GroupBox => (GroupBox)Control;
    
    public GroupBoxViewModel(GroupBox groupBox, Func<string, bool> isIdentifierInUse) : base(groupBox, isIdentifierInUse)
    {
    }
    
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
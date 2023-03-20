using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using RsrcUtilities.Controls;
using RsrcUtilities.RsrcArchitect.ViewModels.Messages;

namespace RsrcUtilities.RsrcArchitect.ViewModels.Controls;

public class CheckBoxViewModel : ControlViewModel
{
    private CheckBox CheckBox => (CheckBox)Control;
    
    public CheckBoxViewModel(CheckBox checkBox, Func<string, bool> isIdentifierInUse) : base(checkBox, isIdentifierInUse)
    {
    }
    
    public string Caption
    {
        get => CheckBox.Caption;
        set
        {
            CheckBox.Caption = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

}
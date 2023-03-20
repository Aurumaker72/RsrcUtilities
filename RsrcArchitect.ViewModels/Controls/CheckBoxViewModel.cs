using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Controls;

public class CheckBoxViewModel : ControlViewModel
{
    public CheckBoxViewModel(CheckBox checkBox, Func<string, bool> isIdentifierInUse) : base(checkBox,
        isIdentifierInUse)
    {
    }

    private CheckBox CheckBox => (CheckBox)Control;

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
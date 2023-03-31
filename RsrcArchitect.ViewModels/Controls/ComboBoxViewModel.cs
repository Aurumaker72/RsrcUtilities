using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Controls;

public class ComboBoxViewModel : ControlViewModel
{
    public ComboBoxViewModel(ComboBox comboBox, Func<string, bool> isIdentifierInUse) : base(comboBox, isIdentifierInUse)
    {
    }
    
    private ComboBox ComboBox => (ComboBox)Control;

    public bool IsSorted
    {
        get => ComboBox.IsSorted;
        set
        {
            ComboBox.IsSorted = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
}
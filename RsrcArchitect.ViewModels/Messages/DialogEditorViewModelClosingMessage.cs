using CommunityToolkit.Mvvm.Messaging.Messages;

namespace RsrcArchitect.ViewModels.Messages;

public class DialogEditorViewModelClosingMessage : ValueChangedMessage<DialogEditorViewModel>
{
    public DialogEditorViewModelClosingMessage(DialogEditorViewModel value) : base(value)
    {
    }
}
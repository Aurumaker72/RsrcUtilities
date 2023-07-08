using CommunityToolkit.Mvvm.Messaging.Messages;

namespace RsrcArchitect.ViewModels.Messages;

public class DialogSavedMessage : ValueChangedMessage<(string Resource, string Header)>
{
    public DialogSavedMessage((string, string) value) : base(value)
    {
    }
}
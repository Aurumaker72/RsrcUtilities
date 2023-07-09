using CommunityToolkit.Mvvm.Messaging.Messages;

namespace RsrcArchitect.ViewModels.Messages;

public class NotificationMessage : ValueChangedMessage<string>
{
    public NotificationMessage(string value) : base(value)
    {
    }
}
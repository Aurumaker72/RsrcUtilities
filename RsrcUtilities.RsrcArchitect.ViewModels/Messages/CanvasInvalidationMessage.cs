using CommunityToolkit.Mvvm.Messaging.Messages;

namespace RsrcUtilities.RsrcArchitect.ViewModels.Messages;

public class CanvasInvalidationMessage : ValueChangedMessage<int>
{
    public CanvasInvalidationMessage(int value) : base(value)
    {        
    }
}
using CommunityToolkit.Mvvm.Messaging.Messages;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Messages;

public class ControlAddingMessage : ValueChangedMessage<Control>
{
    public ControlAddingMessage(Control value) : base(value)
    {
    }
}
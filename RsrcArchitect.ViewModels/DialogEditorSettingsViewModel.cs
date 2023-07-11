using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcArchitect.ViewModels.Types;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorSettingsViewModel : ObservableObject
{
    [ObservableProperty] private int _gripSize = 5;
    [ObservableProperty] private int _gridSize = 10;
    [ObservableProperty] private Positioning _positioning = Positioning.Freeform;
    [ObservableProperty] private string _visualStyle = "nineslice";

    partial void OnGripSizeChanged(int value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    partial void OnGridSizeChanged(int value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    partial void OnPositioningChanged(Positioning value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    partial void OnVisualStyleChanged(string value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
}
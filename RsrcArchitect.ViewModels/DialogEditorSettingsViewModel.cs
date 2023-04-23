using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcArchitect.ViewModels.Types;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorSettingsViewModel : ObservableObject
{
    [ObservableProperty] private float _snapThreshold = 10f;
    [ObservableProperty] private PositioningModes _positioningMode = PositioningModes.Freeform;
    [ObservableProperty] private string _visualStyle = "windows-10";

    partial void OnSnapThresholdChanged(float value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    partial void OnPositioningModeChanged(PositioningModes value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    partial void OnVisualStyleChanged(string value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
}
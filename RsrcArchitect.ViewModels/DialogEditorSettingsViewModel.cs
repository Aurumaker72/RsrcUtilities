using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcArchitect.ViewModels.Types;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorSettingsViewModel : ObservableObject
{
    [ObservableProperty] private float _snapThreshold = 10f;
    [ObservableProperty] private PositioningModes _positioningMode = PositioningModes.Freeform;

    private void OnSnapThresholdChanged() => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    private void OnPositioningModeChanged() => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
}
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcArchitect.ViewModels.Types;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorSettingsViewModel : ObservableObject
{
    [ObservableProperty] private float _snapThreshold = 10f;
    [ObservableProperty] private PositioningModes _positioningMode = PositioningModes.Freeform;
    [ObservableProperty] private string _visualStyle = "nineslice";

    partial void OnSnapThresholdChanged(float value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    partial void OnPositioningModeChanged(PositioningModes value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    partial void OnVisualStyleChanged(string value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
}
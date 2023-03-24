using CommunityToolkit.Mvvm.ComponentModel;
using RsrcArchitect.ViewModels.Types;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorSettingsViewModel : ObservableObject
{
    [ObservableProperty] private float _gripDistance = 10f;
    [ObservableProperty] private PositioningModes _positioningMode = PositioningModes.Freeform;
}
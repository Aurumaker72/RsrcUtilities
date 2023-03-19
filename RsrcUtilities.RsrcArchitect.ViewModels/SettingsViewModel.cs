using CommunityToolkit.Mvvm.ComponentModel;
using RsrcUtilities.RsrcArchitect.ViewModels.Types;

namespace RsrcUtilities.RsrcArchitect.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private float _gripDistance = 10f;
    [ObservableProperty] private PositioningModes _positioningMode = PositioningModes.Arbitrary;
}
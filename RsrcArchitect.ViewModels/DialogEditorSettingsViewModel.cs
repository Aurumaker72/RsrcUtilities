using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcArchitect.ViewModels.Positioners;
using RsrcArchitect.ViewModels.Types;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorSettingsViewModel : ObservableObject
{
    [ObservableProperty] private int _gripSize = 5;
    [ObservableProperty] private int _gridSize = 10;
    [ObservableProperty] private Positioning _positioning = Positioning.Freeform;
    [ObservableProperty] private string _visualStyle = "nineslice";

    internal IPositioner Positioner { get; private set; } = new FreeformPositioner();
    
    partial void OnGripSizeChanged(int value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    partial void OnGridSizeChanged(int value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));

    partial void OnPositioningChanged(Positioning value)
    {
        Positioner = value switch
        {
            Positioning.Freeform => new FreeformPositioner(),
            Positioning.Grid => new GridPositioner()
            {
                SizeFunc = () => GridSize
            },
            Positioning.Snap => new SnapPositioner
            {
                ThresholdFunc = () => GripSize
            },
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    } 
    partial void OnVisualStyleChanged(string value) => WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
}
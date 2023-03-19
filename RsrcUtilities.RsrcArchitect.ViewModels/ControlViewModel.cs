using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.RsrcArchitect.ViewModels.Messages;

namespace RsrcUtilities.RsrcArchitect.ViewModels;

public class ControlViewModel : ObservableObject
{
    private readonly Control _control;

    public ControlViewModel(Control control)
    {
        _control = control;
    }

    public Rectangle Rectangle => _control.Rectangle;
    
    public int X
    {
        get => _control.Rectangle.X;
        set
        {
            _control.Rectangle = _control.Rectangle.WithX(value);
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public int Y
    {
        get => _control.Rectangle.Y;
        set
        {
            _control.Rectangle = _control.Rectangle.WithY(value);
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public int Width
    {
        get => _control.Rectangle.Width;
        set
        {
            _control.Rectangle = _control.Rectangle.WithWidth(value);
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public int Height
    {
        get => _control.Rectangle.Height;
        set
        {
            _control.Rectangle = _control.Rectangle.WithHeight(value);
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
}
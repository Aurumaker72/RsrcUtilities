using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Enums;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.RsrcArchitect.ViewModels.Messages;

namespace RsrcUtilities.RsrcArchitect.ViewModels;

public abstract class ControlViewModel : ObservableObject
{
    protected readonly Control Control;
    private readonly Func<string, bool> _isIdentifierInUse;

    public ControlViewModel(Control control, Func<string, bool> isIdentifierInUse)
    {
        Control = control;
        _isIdentifierInUse = isIdentifierInUse;
    }

    public Rectangle Rectangle => Control.Rectangle;
    
    public string Identifier
    {
        get => Control.Identifier;
        set
        {
            if (!_isIdentifierInUse(value))
            {
                Control.Identifier = value;
            }

            OnPropertyChanged();
        }
    }
    
    public int X
    {
        get => Control.Rectangle.X;
        set
        {
            Control.Rectangle = Control.Rectangle.WithX(value);
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public int Y
    {
        get => Control.Rectangle.Y;
        set
        {
            Control.Rectangle = Control.Rectangle.WithY(value);
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public int Width
    {
        get => Control.Rectangle.Width;
        set
        {
            Control.Rectangle = Control.Rectangle.WithWidth(value);
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public int Height
    {
        get => Control.Rectangle.Height;
        set
        {
            Control.Rectangle = Control.Rectangle.WithHeight(value);
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public HorizontalAlignments HorizontalAlignment
    {
        get => Control.HorizontalAlignment;
        set
        {
            Control.HorizontalAlignment = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public VerticalAlignments VerticalAlignment
    {
        get => Control.VerticalAlignment;
        set
        {
            Control.VerticalAlignment = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
}
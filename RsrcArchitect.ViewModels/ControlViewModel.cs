using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;
using RsrcCore.Geometry;

namespace RsrcArchitect.ViewModels;

public abstract class ControlViewModel : ObservableObject
{
    private readonly Func<string, bool> _isIdentifierInUse;
    internal readonly Control Control;

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
            if (!_isIdentifierInUse(value) && !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value) &&
                !value.Any(char.IsDigit) && value.All(char.IsAscii)) Control.Identifier = value;

            OnPropertyChanged();
        }
    }

    public int X
    {
        get => Control.Rectangle.X;
        set
        {
            Control.Rectangle = Control.Rectangle with { X = value };

            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    public int Y
    {
        get => Control.Rectangle.Y;
        set
        {
            Control.Rectangle = Control.Rectangle with { Y = value };
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    public int Width
    {
        get => Control.Rectangle.Width;
        set
        {
            Control.Rectangle = Control.Rectangle with { Width = value };
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    public int Height
    {
        get => Control.Rectangle.Height;
        set
        {
            Control.Rectangle = Control.Rectangle with { Height = value };
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    public Alignment HorizontalAlignment
    {
        get => Control.HorizontalAlignment;
        set
        {
            Control.HorizontalAlignment = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    public Alignment VerticalAlignment
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
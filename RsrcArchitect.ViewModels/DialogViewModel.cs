using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;
using RsrcCore.Geometry.Structs;
using RsrcCore.Layout.Implementations;
using RsrcCore.Layout.Interfaces;

namespace RsrcArchitect.ViewModels;

public class DialogViewModel : ObservableObject
{
    private readonly ILayoutEngine _layoutEngine;
    private readonly Dialog _dialog;

    public DialogViewModel(Dialog dialog)
    {
        _layoutEngine = new DefaultLayoutEngine();
        _dialog = dialog;
    }

    internal Dialog Dialog => _dialog;

    public Dictionary<Control, Rectangle> DoLayout()
    {
        return _layoutEngine.DoLayout(Dialog);
    }
    
    public int Width
    {
        get => _dialog.Width;
        set
        {
            _dialog.Width = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public int Height
    {
        get => _dialog.Height;
        set
        {
            _dialog.Height = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public string Identifier
    {
        get => _dialog.Identifier;
        set
        {
            _dialog.Identifier = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public string Caption
    {
        get => _dialog.Caption;
        set
        {
            _dialog.Caption = value;
            OnPropertyChanged();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
}
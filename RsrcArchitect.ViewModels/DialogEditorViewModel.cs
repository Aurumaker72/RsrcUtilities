using System.Diagnostics;
using System.Numerics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.Services;
using RsrcArchitect.ViewModels.Factories;
using RsrcArchitect.ViewModels.Helpers;
using RsrcArchitect.ViewModels.Messages;
using RsrcArchitect.ViewModels.Types;
using RsrcCore;
using RsrcCore.Controls;
using RsrcCore.Generators.Implementations;
using RsrcCore.Geometry.Structs;
using RsrcCore.Layout.Implementations;
using RsrcCore.Serializers.Implementations;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorViewModel : ObservableObject, IRecipient<CanvasInvalidationMessage>
{
    private readonly ICanvasInvalidationService _canvasInvalidationService;
    private readonly IFilesService _filesService;
    private readonly SettingsViewModel _settingsViewModel;
    private Grips? _currentGrip;
    private Rectangle _gripStartControlRectangle;
    private Vector2 _gripStartPointerPosition;
    private Vector2 _dragStartPointerPosition;
    private Vector2 _dragStartTranslation;
    private float _zoom = 1f;
    private bool _isDragging;
    private TreeNode<Control>? _selectedNode;

    public DialogEditorViewModel(Dialog dialog, ICanvasInvalidationService canvasInvalidationService,
        IFilesService filesService, SettingsViewModel settingsViewModel)
    {
        _canvasInvalidationService = canvasInvalidationService;
        _filesService = filesService;
        _settingsViewModel = settingsViewModel;
        Dialog = dialog;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    private TreeNode<Control>? SelectedNode
    {
        get => _selectedNode;
        set
        {
            SetProperty(ref _selectedNode, value);
            OnPropertyChanged(nameof(IsNodeSelected));
            DeleteSelectedNodeCommand.NotifyCanExecuteChanged();
            BringSelectedNodeToFrontCommand.NotifyCanExecuteChanged();
            // TODO: cache the viewmodels, since creation creates garbage
            SelectedControlViewModel = value != null
                ? ControlViewModelFactory.Create(value.Data,
                    s =>
                    {
                        return Dialog.Root.Flatten().Any(x =>
                            x.Identifier.Equals(s, StringComparison.InvariantCultureIgnoreCase));
                    })
                : null;
            OnPropertyChanged(nameof(SelectedControlViewModel));
        }
    }

    public float Zoom
    {
        get => _zoom;
        set
        {
            SetProperty(ref _zoom, Math.Max(0.25f, value));
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }
    
    public bool IsNodeSelected => SelectedNode != null;
    public ControlViewModel? SelectedControlViewModel { get; private set; }
    public Vector2 Translation { get; private set; } = Vector2.Zero;
    public Dialog Dialog { get; }

    #region Interface Methods

    void IRecipient<CanvasInvalidationMessage>.Receive(CanvasInvalidationMessage message)
    {
        _canvasInvalidationService.Invalidate();
    }

    #endregion
    

    #region Private Methods

    private bool TryCreateControlFromName(out Control? control, string tool)
    {
        control = tool switch
        {
            "Button" => new Button(),
            "TextBox" => new TextBox(),
            "CheckBox" => new CheckBox(),
            "GroupBox" => new GroupBox(),
            _ => null
        };

        return control != null;
    }


    private Grips? GetGrip(Control control, Vector2 position)
    {
        Debug.Print($"Position: {position} | Control X/Y: {control.Rectangle.X} {control.Rectangle.Y}");
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.X, control.Rectangle.Y)) < _settingsViewModel.GripDistance)
            return Grips.TopLeft;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.Right, control.Rectangle.Y)) < _settingsViewModel.GripDistance)
            return Grips.TopRight;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.X, control.Rectangle.Bottom)) < _settingsViewModel.GripDistance)
            return Grips.BottomLeft;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.Right, control.Rectangle.Bottom)) <
            _settingsViewModel.GripDistance)
            return Grips.BottomRight;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.X, control.Rectangle.CenterY)) <
            _settingsViewModel.GripDistance)
            return Grips.Left;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.CenterX, control.Rectangle.Y)) <
            _settingsViewModel.GripDistance)
            return Grips.Top;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.CenterX, control.Rectangle.Bottom)) <
            _settingsViewModel.GripDistance)
            return Grips.Bottom;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.Right, control.Rectangle.CenterY)) <
            _settingsViewModel.GripDistance)
            return Grips.Right;
        if (control.Rectangle.Contains(new Vector2Int(position)))
            return Grips.Move;
        return null;
    }

    private Vector2 ProcessPosition(Vector2 vector2)
    {
        switch (_settingsViewModel.PositioningMode)
        {
            case PositioningModes.Arbitrary:
                return vector2;
            case PositioningModes.Grid:
                const int coarseness = 10;
                return new Vector2((float)(Math.Round(vector2.X / coarseness) * coarseness),
                    (float)(Math.Round(vector2.Y / coarseness) * coarseness));
            default:
                throw new NotImplementedException();
        }
    }

    private TreeNode<Control>? GetControlNodeAtPosition(Vector2 position)
    {
        // TODO: make special case for groupboxes; the center is clickthrough
        return Dialog.Root.Reverse().FirstOrDefault(node =>
            position.X > node.Data.Rectangle.X && position.Y > node.Data.Rectangle.Y &&
            position.X < node.Data.Rectangle.Right && position.Y < node.Data.Rectangle.Bottom);
    }

    private Vector2 RelativePositionToDialog(Vector2 position)
    {
        return (position - Translation) / Zoom;
    }
    
    #endregion

    #region Commands

    [RelayCommand]
    private void CreateControl(string tool)
    {
        if (TryCreateControlFromName(out var control, tool))
        {
            control.Identifier = StringHelper.GetRandomAlphabeticString(16);
            var size = new Vector2Int(90, 25);
            
            control.Rectangle = new Rectangle(Dialog.Width / 2 - size.X / 2, Dialog.Height / 2 - size.Y / 2, size.X, size.Y);

            Dialog.Root.AddChild(control);
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    [RelayCommand]
    private void PointerPress(Vector2 position)
    {
        var dialogPosition = RelativePositionToDialog(position);
        
        // do grip-test first, because clicks outside of control bounds can grip too
        var isGripHit = false;
        if (SelectedNode != null) isGripHit = GetGrip(SelectedNode.Data, dialogPosition) != null;

        if (!isGripHit) SelectedNode = GetControlNodeAtPosition(dialogPosition);

        if (SelectedNode != null)
        {
            _currentGrip = GetGrip(SelectedNode.Data, dialogPosition);
            _gripStartControlRectangle = SelectedNode.Data.Rectangle;
            _gripStartPointerPosition = ProcessPosition(dialogPosition);
        }
        else
        {
            _isDragging = true;
            _dragStartPointerPosition = position;
            _dragStartTranslation = Translation;
        }

        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand]
    private void PointerRelease()
    {
        _isDragging = false;
        _currentGrip = null;
    }

    [RelayCommand]
    private void PointerMove(Vector2 position)
    {
        if (_isDragging)
        {
            Translation = _dragStartTranslation + (position - _dragStartPointerPosition);
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
            return;
        }
        
        var dialogPosition = ProcessPosition(RelativePositionToDialog(position));

        if (SelectedControlViewModel != null)
            switch (_currentGrip)
            {
                case Grips.TopLeft:
                    dialogPosition.X = Math.Min(dialogPosition.X, (float)_gripStartControlRectangle.Right);
                    dialogPosition.Y = Math.Min(dialogPosition.Y, (float)_gripStartControlRectangle.Bottom);
                    SelectedControlViewModel.X = (int)dialogPosition.X;
                    SelectedControlViewModel.Y = (int)dialogPosition.Y;
                    SelectedControlViewModel.Width = (int)(_gripStartControlRectangle.Width +
                                                           (_gripStartPointerPosition.X - dialogPosition.X));
                    SelectedControlViewModel.Height = (int)(_gripStartControlRectangle.Height +
                                                            (_gripStartPointerPosition.Y - dialogPosition.Y));
                    break;
                case Grips.BottomLeft:
                    dialogPosition.X = Math.Min(dialogPosition.X, (float)_gripStartControlRectangle.Right);

                    SelectedControlViewModel.X = (int)dialogPosition.X;
                    SelectedControlViewModel.Width =
                        (int)(_gripStartControlRectangle.Width + (_gripStartPointerPosition.X - dialogPosition.X));
                    SelectedControlViewModel.Height = (int)Math.Max(0,
                        _gripStartControlRectangle.Height + (dialogPosition.Y - _gripStartPointerPosition.Y));

                    break;
                case Grips.BottomRight:
                    dialogPosition.X = Math.Max(dialogPosition.X, (float)_gripStartControlRectangle.X);
                    dialogPosition.Y = Math.Max(dialogPosition.Y, (float)_gripStartControlRectangle.Y);

                    SelectedControlViewModel.Width =
                        (int)(_gripStartControlRectangle.Width + (dialogPosition.X - _gripStartPointerPosition.X));
                    SelectedControlViewModel.Height =
                        (int)(_gripStartControlRectangle.Height + (dialogPosition.Y - _gripStartPointerPosition.Y));
                    break;
                case Grips.TopRight:
                    dialogPosition.X = Math.Max(dialogPosition.X, (float)_gripStartControlRectangle.X);
                    dialogPosition.Y = Math.Min(dialogPosition.Y, (float)_gripStartControlRectangle.Bottom);

                    SelectedControlViewModel.Y = (int)dialogPosition.Y;
                    SelectedControlViewModel.Width =
                        (int)(_gripStartControlRectangle.Width + (dialogPosition.X - _gripStartPointerPosition.X));
                    SelectedControlViewModel.Height =
                        (int)(_gripStartControlRectangle.Height + (_gripStartPointerPosition.Y - dialogPosition.Y));
                    break;
                case Grips.Left:
                    dialogPosition.X = Math.Min(dialogPosition.X, (float)_gripStartControlRectangle.Right);

                    SelectedControlViewModel.X = (int)dialogPosition.X;
                    SelectedControlViewModel.Width = (int)(_gripStartControlRectangle.Width +
                                                           (_gripStartPointerPosition.X - dialogPosition.X));
                    break;
                case Grips.Right:
                    dialogPosition.X = Math.Max(dialogPosition.X, (float)_gripStartControlRectangle.X);
                    SelectedControlViewModel.Width =
                        (int)(_gripStartControlRectangle.Width + (dialogPosition.X - _gripStartPointerPosition.X));
                    break;
                case Grips.Bottom:
                    dialogPosition.Y = Math.Max(dialogPosition.Y, (float)_gripStartControlRectangle.Y);

                    SelectedControlViewModel.Height =
                        (int)(_gripStartControlRectangle.Height + (dialogPosition.Y - _gripStartPointerPosition.Y));

                    break;
                case Grips.Top:
                    dialogPosition.Y = Math.Min(dialogPosition.Y, (float)_gripStartControlRectangle.Bottom);

                    SelectedControlViewModel.Y = (int)dialogPosition.Y;
                    SelectedControlViewModel.Height = (int)(_gripStartControlRectangle.Height +
                                                            (_gripStartPointerPosition.Y - dialogPosition.Y));
                    break;
                case Grips.Move:
                    SelectedControlViewModel.X =
                        (int)(_gripStartControlRectangle.X + (dialogPosition.X - _gripStartPointerPosition.X));
                    SelectedControlViewModel.Y =
                        (int)(_gripStartControlRectangle.Y + (dialogPosition.Y - _gripStartPointerPosition.Y));
                    break;
                case null:
                    break;
            }
    }

    [RelayCommand(CanExecute = nameof(IsNodeSelected))]
    private void DeleteSelectedNode()
    {
        Dialog.Root.Children.Remove(SelectedNode!);
        SelectedNode = null;
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand(CanExecute = nameof(IsNodeSelected))]
    private void BringSelectedNodeToFront()
    {
        Dialog.Root.Children.Remove(SelectedNode!);
        Dialog.Root.Children.Add(SelectedNode!);
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand]
    private async Task Save()
    {
        var serializedDialog = new RcDialogSerializer().Serialize(
            new DefaultLayoutEngine().DoLayout(Dialog), Dialog);
        var generatedHeader = new CxxHeaderResourceGenerator().Generate(Dialog.Root.Flatten());

        var resourceFile =
            await _filesService.TryPickSaveFileAsync("rsrc_snippet.rc", ("Resource File", new[] { "rc" }));
        var headerFile = await _filesService.TryPickSaveFileAsync("resource.h", ("C/C++ Header File", new[] { "h" }));

        if (resourceFile == null || headerFile == null) return;

        await using var resourceStream = await resourceFile.OpenStreamForWriteAsync();
        await using var headerStream = await headerFile.OpenStreamForWriteAsync();

        resourceStream.Write(Encoding.Default.GetBytes((string)serializedDialog));
        await resourceStream.FlushAsync();

        headerStream.Write(Encoding.Default.GetBytes((string)generatedHeader));
        await headerStream.FlushAsync();
    }

    #endregion
}
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

public partial class DialogEditorViewModel : ObservableObject
{
    private readonly IFilesService _filesService;
    private Grips? _currentGrip;
    private Rectangle _gripStartControlRectangle;
    private Vector2 _gripStartPointerPosition;
    private Vector2 _dragStartPointerPosition;
    private Vector2 _dragStartTranslation;
    private float _scale = 1f;
    private bool _isDragging;
    private TreeNode<Control>? _selectedNode;

    public DialogEditorViewModel(Dialog dialog,
        IFilesService filesService, string friendlyName)
    {
        _filesService = filesService;
        FriendlyName = friendlyName;
        DialogViewModel = new DialogViewModel(dialog);
        ToolboxItemViewModels = new List<ToolboxItemViewModel>
        {
            new(this, "button", () => new Button()),
            new(this, "textbox", () => new TextBox()),
            new(this, "checkbox", () => new CheckBox()),
            new(this, "groupbox", () => new GroupBox())
        };
        DialogEditorSettingsViewModel = new();
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public List<ToolboxItemViewModel> ToolboxItemViewModels { get; }
    public DialogViewModel DialogViewModel { get; }
    public DialogEditorSettingsViewModel DialogEditorSettingsViewModel { get; }
    public string FriendlyName { get; }
    
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
                        return DialogViewModel.Dialog.Root.Flatten().Any(x =>
                            x.Identifier.Equals(s, StringComparison.InvariantCultureIgnoreCase));
                    })
                : null;
            OnPropertyChanged(nameof(SelectedControlViewModel));
        }
    }

    public float Scale
    {
        get => _scale;
        set
        {
            SetProperty(ref _scale, Math.Max(0.25f, value));
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    public bool IsNodeSelected => SelectedNode != null;
    public ControlViewModel? SelectedControlViewModel { get; private set; }
    public Vector2 Translation { get; private set; } = Vector2.Zero;


    #region Private Methods

    private Grips? GetGrip(Control control, Vector2 position)
    {
        // grip distance is the same as snap threshold
        
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.X, control.Rectangle.Y)) < DialogEditorSettingsViewModel.SnapThreshold)
            return Grips.TopLeft;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.Right, control.Rectangle.Y)) < DialogEditorSettingsViewModel.SnapThreshold)
            return Grips.TopRight;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.X, control.Rectangle.Bottom)) < DialogEditorSettingsViewModel.SnapThreshold)
            return Grips.BottomLeft;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.Right, control.Rectangle.Bottom)) <
            DialogEditorSettingsViewModel.SnapThreshold)
            return Grips.BottomRight;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.X, control.Rectangle.CenterY)) <
            DialogEditorSettingsViewModel.SnapThreshold)
            return Grips.Left;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.CenterX, control.Rectangle.Y)) <
            DialogEditorSettingsViewModel.SnapThreshold)
            return Grips.Top;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.CenterX, control.Rectangle.Bottom)) <
            DialogEditorSettingsViewModel.SnapThreshold)
            return Grips.Bottom;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.Right, control.Rectangle.CenterY)) <
            DialogEditorSettingsViewModel.SnapThreshold)
            return Grips.Right;
        if (control.Rectangle.Contains(new Vector2Int(position)))
            return Grips.Move;
        return null;
    }

    // process a target control's rectangle
    private Rectangle ProcessRectangle(IEnumerable<TreeNode<Control>> controls, Control targetControl)
    {
        switch (DialogEditorSettingsViewModel.PositioningMode)
        {
            case PositioningModes.Freeform:
                return targetControl.Rectangle;
            case PositioningModes.Grid:
                // TODO: make this adjustable in the DialogEditorSettingsViewModel
                return new Rectangle((int)(Math.Round(targetControl.Rectangle.X / DialogEditorSettingsViewModel.SnapThreshold) * DialogEditorSettingsViewModel.SnapThreshold),
                    (int)(Math.Round(targetControl.Rectangle.Y / DialogEditorSettingsViewModel.SnapThreshold) * DialogEditorSettingsViewModel.SnapThreshold), targetControl.Rectangle.Width, targetControl.Rectangle.Height);
            case PositioningModes.Snap:
                
                // TODO: optimize by utilizing a LUT instead?

                bool hasSnappedX = false;
                bool hasSnappedY = false;
                
                // enumerate all other controls
                foreach (var node in controls.Where(x => !x.Data.Identifier.Equals(targetControl.Identifier)))
                {
                    if (!hasSnappedX && Math.Abs(node.Data.Rectangle.X - targetControl.Rectangle.X) < DialogEditorSettingsViewModel.SnapThreshold)
                    {
                        targetControl.Rectangle = targetControl.Rectangle.WithX(node.Data.Rectangle.X);
                        hasSnappedX = true;
                    }
                    if (!hasSnappedX && Math.Abs(node.Data.Rectangle.Right - targetControl.Rectangle.Right) < DialogEditorSettingsViewModel.SnapThreshold)
                    {
                        targetControl.Rectangle = targetControl.Rectangle.WithX(node.Data.Rectangle.Right - targetControl.Rectangle.Width);
                        hasSnappedX = true;
                    }
                    if (!hasSnappedY && Math.Abs(node.Data.Rectangle.Y - targetControl.Rectangle.Y) < DialogEditorSettingsViewModel.SnapThreshold)
                    {
                        targetControl.Rectangle = targetControl.Rectangle.WithY(node.Data.Rectangle.Y);
                        hasSnappedY = true;
                    }
                    if (!hasSnappedY && Math.Abs(node.Data.Rectangle.Bottom - targetControl.Rectangle.Bottom) < DialogEditorSettingsViewModel.SnapThreshold)
                    {
                        targetControl.Rectangle = targetControl.Rectangle.WithY(node.Data.Rectangle.Bottom - targetControl.Rectangle.Height);
                        hasSnappedY = true;
                    }
                }
                
                return targetControl.Rectangle;
            default:
                throw new NotImplementedException();
        }
    }

    private TreeNode<Control>? GetControlNodeAtPosition(Vector2 position)
    {
        // TODO: make special case for groupboxes; the center is clickthrough
        return DialogViewModel.Dialog.Root.Reverse().FirstOrDefault(node =>
            position.X > node.Data.Rectangle.X && position.Y > node.Data.Rectangle.Y &&
            position.X < node.Data.Rectangle.Right && position.Y < node.Data.Rectangle.Bottom);
    }

    private Vector2 RelativePositionToDialog(Vector2 position)
    {
        return (position - Translation) / Scale;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Close()
    {
        WeakReferenceMessenger.Default.Send(new DialogEditorViewModelClosingMessage(this));
    }
    
    [RelayCommand]
    private void PointerPress(Vector2 position)
    {
        var dialogPosition = RelativePositionToDialog(position);

        // do grip-test first, then store if it hits
        var isGripHit = false;
        if (SelectedNode != null) isGripHit = GetGrip(SelectedNode.Data, dialogPosition) != null;

        // if no grip hits, we know we aren't starting to resize or move a control,
        // thus we can select a new one
        if (!isGripHit) SelectedNode = GetControlNodeAtPosition(dialogPosition);

        if (SelectedNode != null)
        {
            _currentGrip = GetGrip(SelectedNode.Data, dialogPosition);
            _gripStartControlRectangle = SelectedNode.Data.Rectangle;
            _gripStartPointerPosition = dialogPosition;
        }
        else
        {
            // no node caught but stil clicked,
            // so start translating the camera
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


        if (SelectedControlViewModel == null) return;

        position = RelativePositionToDialog(position);
        
        switch (_currentGrip)
        {
            case Grips.TopLeft:
                position.X = Math.Min(position.X, _gripStartControlRectangle.Right);
                position.Y = Math.Min(position.Y, _gripStartControlRectangle.Bottom);
                SelectedControlViewModel.X = (int)position.X;
                SelectedControlViewModel.Y = (int)position.Y;
                SelectedControlViewModel.Width = (int)(_gripStartControlRectangle.Width +
                                                       (_gripStartPointerPosition.X - position.X));
                SelectedControlViewModel.Height = (int)(_gripStartControlRectangle.Height +
                                                        (_gripStartPointerPosition.Y - position.Y));
                break;
            case Grips.BottomLeft:
                position.X = Math.Min(position.X, _gripStartControlRectangle.Right);

                SelectedControlViewModel.X = (int)position.X;
                SelectedControlViewModel.Width =
                    (int)(_gripStartControlRectangle.Width + (_gripStartPointerPosition.X - position.X));
                SelectedControlViewModel.Height = (int)Math.Max(0,
                    _gripStartControlRectangle.Height + (position.Y - _gripStartPointerPosition.Y));

                break;
            case Grips.BottomRight:
                position.X = Math.Max(position.X, _gripStartControlRectangle.X);
                position.Y = Math.Max(position.Y, _gripStartControlRectangle.Y);

                SelectedControlViewModel.Width =
                    (int)(_gripStartControlRectangle.Width + (position.X - _gripStartPointerPosition.X));
                SelectedControlViewModel.Height =
                    (int)(_gripStartControlRectangle.Height + (position.Y - _gripStartPointerPosition.Y));
                break;
            case Grips.TopRight:
                position.X = Math.Max(position.X, _gripStartControlRectangle.X);
                position.Y = Math.Min(position.Y, _gripStartControlRectangle.Bottom);

                SelectedControlViewModel.Y = (int)position.Y;
                SelectedControlViewModel.Width =
                    (int)(_gripStartControlRectangle.Width + (position.X - _gripStartPointerPosition.X));
                SelectedControlViewModel.Height =
                    (int)(_gripStartControlRectangle.Height + (_gripStartPointerPosition.Y - position.Y));
                break;
            case Grips.Left:
                position.X = Math.Min(position.X, _gripStartControlRectangle.Right);

                SelectedControlViewModel.X = (int)position.X;
                SelectedControlViewModel.Width = (int)(_gripStartControlRectangle.Width +
                                                       (_gripStartPointerPosition.X - position.X));
                break;
            case Grips.Right:
                position.X = Math.Max(position.X, _gripStartControlRectangle.X);
                SelectedControlViewModel.Width =
                    (int)(_gripStartControlRectangle.Width + (position.X - _gripStartPointerPosition.X));
                break;
            case Grips.Bottom:
                position.Y = Math.Max(position.Y, _gripStartControlRectangle.Y);

                SelectedControlViewModel.Height =
                    (int)(_gripStartControlRectangle.Height + (position.Y - _gripStartPointerPosition.Y));

                break;
            case Grips.Top:
                position.Y = Math.Min(position.Y, _gripStartControlRectangle.Bottom);

                SelectedControlViewModel.Y = (int)position.Y;
                SelectedControlViewModel.Height = (int)(_gripStartControlRectangle.Height +
                                                        (_gripStartPointerPosition.Y - position.Y));
                break;
            case Grips.Move:
                SelectedControlViewModel.X =
                    (int)(_gripStartControlRectangle.X + (position.X - _gripStartPointerPosition.X));
                SelectedControlViewModel.Y =
                    (int)(_gripStartControlRectangle.Y + (position.Y - _gripStartPointerPosition.Y));
                break;
            case null:
                break;
        }

        var processedRectangle = ProcessRectangle(DialogViewModel.Dialog.Root, SelectedControlViewModel.Control);
        SelectedControlViewModel.X = processedRectangle.X;
        SelectedControlViewModel.Y = processedRectangle.Y;
        SelectedControlViewModel.Width = processedRectangle.Width;
        SelectedControlViewModel.Height = processedRectangle.Height;
        
        
    }

    [RelayCommand(CanExecute = nameof(IsNodeSelected))]
    private void DeleteSelectedNode()
    {
        DialogViewModel.Dialog.Root.Children.Remove(SelectedNode!);
        SelectedNode = null;
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand(CanExecute = nameof(IsNodeSelected))]
    private void BringSelectedNodeToFront()
    {
        DialogViewModel.Dialog.Root.Children.Remove(SelectedNode!);
        DialogViewModel.Dialog.Root.Children.Add(SelectedNode!);
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand]
    private async Task Save()
    {
        var serializedDialog = new RcDialogSerializer().Serialize(
            new DefaultLayoutEngine().DoLayout(DialogViewModel.Dialog), DialogViewModel.Dialog);
        var generatedHeader = new CxxHeaderInformationGenerator().Generate(DialogViewModel.Dialog.Root.Flatten());

        var resourceFile =
            await _filesService.TryPickSaveFileAsync("rsrc_snippet.rc", ("Resource File", new[] { "rc" }));
        var headerFile = await _filesService.TryPickSaveFileAsync("resource.h", ("C/C++ Header File", new[] { "h" }));

        if (resourceFile == null || headerFile == null) return;

        await using var resourceStream = await resourceFile.OpenStreamForWriteAsync();
        await using var headerStream = await headerFile.OpenStreamForWriteAsync();

        resourceStream.Write(Encoding.Default.GetBytes(serializedDialog));
        await resourceStream.FlushAsync();

        headerStream.Write(Encoding.Default.GetBytes(generatedHeader));
        await headerStream.FlushAsync();
    }

    #endregion



    internal void AddControl(Control control)
    {
		// generate randomized unique identifier because we can't have duplicate identifiers
		control.Identifier = StringHelper.GetRandomAlphabeticString(16);

		// place it roughly in the middle
		var size = new Vector2Int(90, 25);
		control.Rectangle = new Rectangle(DialogViewModel.Dialog.Width / 2 - size.X,
			DialogViewModel.Dialog.Height / 2 - size.Y, size.X, size.Y);

		// add it to the root node, then queue an invalidation
		DialogViewModel.Dialog.Root.AddChild(control);
		WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
	}
}
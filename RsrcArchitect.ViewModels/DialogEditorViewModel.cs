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
    private readonly SettingsViewModel _settingsViewModel;
    private Grips? _currentGrip;
    private Rectangle _gripStartControlRectangle;
    private Vector2 _gripStartPointerPosition;
    private Vector2 _dragStartPointerPosition;
    private Vector2 _dragStartTranslation;
    private float _zoom = 1f;
    private bool _isDragging;
    private TreeNode<Control>? _selectedNode;

    public DialogEditorViewModel(Dialog dialog,
        IFilesService filesService, SettingsViewModel settingsViewModel, string friendlyName)
    {
        _filesService = filesService;
        _settingsViewModel = settingsViewModel;
        FriendlyName = friendlyName;
        DialogViewModel = new DialogViewModel(dialog);
        ToolboxItemViewModels = new List<ToolboxItemViewModel>
        {
            new(this, "button", () => new Button()),
            new(this, "textbox", () => new TextBox()),
            new(this, "checkbox", () => new CheckBox()),
            new(this, "groupbox", () => new GroupBox())
        };
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public List<ToolboxItemViewModel> ToolboxItemViewModels { get; }
    public DialogViewModel DialogViewModel { get; }
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


    #region Private Methods

    private Grips? GetGrip(Control control, Vector2 position)
    {
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

    private Vector2 ProcessPosition(IEnumerable<TreeNode<Control>> controls, Control selectedControl, Vector2 vector2)
    {
        switch (_settingsViewModel.PositioningMode)
        {
            case PositioningModes.Freeform:
                return vector2;
            case PositioningModes.Grid:
                // TODO: make this adjustable in the SettingsViewModel
                const int coarseness = 10;
                return new Vector2((float)(Math.Round(vector2.X / coarseness) * coarseness),
                    (float)(Math.Round(vector2.Y / coarseness) * coarseness));
            case PositioningModes.Snap:

                // aggregate all coordinates of other controls
                var xCoordinates = controls.Where(x => !x.Data.Identifier.Equals(selectedControl.Identifier))
                    .Select(node => node.Data.Rectangle.X);
                var yCoordinates = controls.Where(x => !x.Data.Identifier.Equals(selectedControl.Identifier))
                    .Select(node => node.Data.Rectangle.Y);

                // TODO: optimize this
                foreach (var x in xCoordinates)
                {
                    if (Math.Abs(selectedControl.Rectangle.X - x) >= 10) continue;
                    
                    vector2.X = selectedControl.Rectangle.X;
                    break;
                }
                
                foreach (var y in yCoordinates)
                {
                    if (Math.Abs(selectedControl.Rectangle.Y - y) >= 10) continue;
                    
                    vector2.Y = selectedControl.Rectangle.Y;
                    break;
                }
                
                return vector2;
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
        return (position - Translation) / Zoom;
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
            _gripStartPointerPosition = ProcessPosition(DialogViewModel.Dialog.Root, SelectedNode.Data, dialogPosition);
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


        if (SelectedControlViewModel != null)
        {
            position = ProcessPosition(DialogViewModel.Dialog.Root, SelectedNode.Data, RelativePositionToDialog(position));

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
        }
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
        var generatedHeader = new CxxHeaderResourceGenerator().Generate(DialogViewModel.Dialog.Root.Flatten());

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
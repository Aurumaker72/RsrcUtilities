﻿using System.Numerics;
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
using RsrcCore.Geometry;
using RsrcCore.Layout.Implementations;
using RsrcCore.Serializers.Implementations;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorViewModel : ObservableObject
{
    private readonly IFilePickerService _filePickerService;
    private readonly DialogEditorSettingsViewModel _dialogEditorSettingsViewModel;

    private Transformation _transformation = Transformation.None;
    private Sizing _sizing = Sizing.Empty;

    private Rectangle _transformationStartRectangle;
    private Vector2 _transformationStartPointerPosition;
    private Vector2 _panStartPointerPosition;
    private Vector2 _panStartTranslation;
    private float _scale = 1f;
    private bool _isPanning;
    private TreeNode<Control>? _selectedNode;

    public DialogEditorViewModel(Dialog dialog, string friendlyName,
        DialogEditorSettingsViewModel dialogEditorSettingsViewModel, IFilePickerService filePickerService)
    {
        FriendlyName = friendlyName;
        _dialogEditorSettingsViewModel = dialogEditorSettingsViewModel;
        _filePickerService = filePickerService;
        DialogViewModel = new DialogViewModel(dialog);
        ToolboxItemViewModels = new List<ToolboxItemViewModel>
        {
            new(this, "button", () => new Button()),
            new(this, "textbox", () => new TextBox()),
            new(this, "checkbox", () => new CheckBox()),
            new(this, "groupbox", () => new GroupBox()),
            new(this, "combobox", () => new ComboBox()),
            new(this, "label", () => new Label())
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

    private (Transformation transformation, Sizing sizing) GetTransformationOperationCandidate(Control control,
        Vector2 position)
    {
        var relative = position - control.Rectangle.ToVector2();

        var transformation = Transformation.Size;
        var sizing = Sizing.Empty;

        sizing = sizing with { Left = Math.Abs(relative.X - 0) < _dialogEditorSettingsViewModel.GripSize };
        sizing = sizing with { Top = Math.Abs(relative.Y - 0) < _dialogEditorSettingsViewModel.GripSize };
        sizing = sizing with
        {
            Right = Math.Abs(relative.X - control.Rectangle.Width) < _dialogEditorSettingsViewModel.GripSize
        };
        sizing = sizing with
        {
            Bottom = Math.Abs(relative.Y - control.Rectangle.Height) < _dialogEditorSettingsViewModel.GripSize
        };

        if (sizing.IsEmpty)
        {
            transformation = Transformation.Move;
        }

        if (!control.Rectangle.Inflate(_dialogEditorSettingsViewModel.GripSize).Contains(new(position)))
        {
            transformation = Transformation.None;
        }

        return (transformation, sizing);
    }

    // process a target control's rectangle
    private Rectangle ProcessRectangle(IEnumerable<TreeNode<Control>> controls, Control targetControl)
    {
        switch (_dialogEditorSettingsViewModel.Positioning)
        {
            case Positioning.Freeform:
                return targetControl.Rectangle;
            case Positioning.Grid:

                int Snap(int value, float to)
                {
                    return (int)(Math.Round(value / to) * to);
                }


                return new Rectangle(Snap(targetControl.Rectangle.X, _dialogEditorSettingsViewModel.GridSize),
                    Snap(targetControl.Rectangle.Y, _dialogEditorSettingsViewModel.GridSize),
                    Snap(targetControl.Rectangle.Width, _dialogEditorSettingsViewModel.GridSize),
                    Snap(targetControl.Rectangle.Height, _dialogEditorSettingsViewModel.GridSize));
            case Positioning.Snap:

                // TODO: optimize by utilizing a LUT instead?

                var hasSnappedX = false;
                var hasSnappedY = false;

                // enumerate all other controls
                foreach (var node in controls.Where(x => !x.Data.Identifier.Equals(targetControl.Identifier)))
                {
                    if (!hasSnappedX && Math.Abs(node.Data.Rectangle.X - targetControl.Rectangle.X) <
                        _dialogEditorSettingsViewModel.GridSize)
                    {
                        targetControl.Rectangle = targetControl.Rectangle with { X = node.Data.Rectangle.X };
                        hasSnappedX = true;
                    }

                    if (!hasSnappedX && Math.Abs(node.Data.Rectangle.Right - targetControl.Rectangle.Right) <
                        _dialogEditorSettingsViewModel.GridSize)
                    {
                        targetControl.Rectangle = targetControl.Rectangle with
                        {
                            X = node.Data.Rectangle.Right - targetControl.Rectangle.Width
                        };
                        hasSnappedX = true;
                    }

                    if (!hasSnappedY && Math.Abs(node.Data.Rectangle.Y - targetControl.Rectangle.Y) <
                        _dialogEditorSettingsViewModel.GridSize)
                    {
                        targetControl.Rectangle = targetControl.Rectangle with { Y = node.Data.Rectangle.Y };
                        hasSnappedY = true;
                    }

                    if (!hasSnappedY && Math.Abs(node.Data.Rectangle.Bottom - targetControl.Rectangle.Bottom) <
                        _dialogEditorSettingsViewModel.GridSize)
                    {
                        targetControl.Rectangle = targetControl.Rectangle with
                        {
                            Y = node.Data.Rectangle.Bottom - targetControl.Rectangle.Height
                        };
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
        if (SelectedNode != null)
            isGripHit = GetTransformationOperationCandidate(SelectedNode.Data, dialogPosition).transformation !=
                        Transformation.None;

        // if no grip hits, we know we aren't starting to resize or move a control,
        // thus we can select a new one
        if (!isGripHit) SelectedNode = GetControlNodeAtPosition(dialogPosition);

        if (SelectedNode != null)
        {
            (_transformation, _sizing) = GetTransformationOperationCandidate(SelectedNode.Data, dialogPosition);
            _transformationStartRectangle = SelectedNode.Data.Rectangle;
            _transformationStartPointerPosition = dialogPosition;
        }
        else
        {
            // no node caught but stil clicked,
            // so start translating the camera
            _isPanning = true;
            _panStartPointerPosition = position;
            _panStartTranslation = Translation;
        }

        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand]
    private void PointerRelease()
    {
        _isPanning = false;
        _transformation = Transformation.None;
        _sizing = Sizing.Empty;
    }

    [RelayCommand]
    private void PointerMove(Vector2 position)
    {
        if (_isPanning)
        {
            Translation = _panStartTranslation + (position - _panStartPointerPosition);
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
            return;
        }


        if (SelectedControlViewModel == null) return;

        position = RelativePositionToDialog(position);

        switch (_transformation)
        {
            case Transformation.Size:
                if (_sizing.Left)
                {
                    position.X = Math.Min(position.X, _transformationStartRectangle.Right);
                    SelectedControlViewModel.X = (int)position.X;
                    SelectedControlViewModel.Width =
                        _transformationStartRectangle.Right - SelectedControlViewModel.X;
                }

                if (_sizing.Top)
                {
                    position.Y = Math.Min(position.Y, _transformationStartRectangle.Bottom);
                    SelectedControlViewModel.Y = (int)position.Y;
                    SelectedControlViewModel.Height =
                        _transformationStartRectangle.Bottom - SelectedControlViewModel.Y;
                }

                if (_sizing.Right)
                {
                    position.X = Math.Max(position.X, _transformationStartRectangle.X);
                    SelectedControlViewModel.Width = (int)(_transformationStartRectangle.Width +
                                                           (position.X - _transformationStartRectangle.Right));
                }

                if (_sizing.Bottom)
                {
                    position.Y = Math.Max(position.Y, _transformationStartRectangle.Y);
                    SelectedControlViewModel.Height = (int)(_transformationStartRectangle.Height +
                                                            (position.Y - _transformationStartRectangle.Bottom));
                }

                break;
            case Transformation.Move:
                SelectedControlViewModel.X =
                    (int)(_transformationStartRectangle.X + (position.X - _transformationStartPointerPosition.X));
                SelectedControlViewModel.Y =
                    (int)(_transformationStartRectangle.Y + (position.Y - _transformationStartPointerPosition.Y));
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
    private async Task SaveRc()
    {
        var rc = new RcDialogSerializer()
        {
            GenerateCompilable = true
        }.Serialize(
            new DefaultLayoutEngine().DoLayout(DialogViewModel.Dialog), DialogViewModel.Dialog);
        var header = new CxxHeaderInformationGenerator().Generate(DialogViewModel.Dialog);

        var resourceFile =
            await _filePickerService.TryPickSaveFileAsync("rsrc.rc", ("Resource File", new[] { "rc" }));
        var headerFile =
            await _filePickerService.TryPickSaveFileAsync("resource.h", ("C/C++ Header File", new[] { "h" }));

        if (resourceFile == null || headerFile == null) return;

        await File.WriteAllTextAsync(resourceFile, rc);
        await File.WriteAllTextAsync(headerFile, header);
    }

    [RelayCommand]
    private async Task SaveUgui()
    {
        var lua = new UguiDialogSerializer().Serialize(new DefaultLayoutEngine().DoLayout(DialogViewModel.Dialog),
            DialogViewModel.Dialog);

        var luaFile =
            await _filePickerService.TryPickSaveFileAsync("ugui.lua", ("Lua Script File", new[] { "lua" }));

        if (luaFile == null)
        {
            return;
        }

        await File.WriteAllTextAsync(luaFile, lua);
    }

    #endregion


    internal void AddControl(Control control)
    {
        // generate randomized unique identifier because we can't have duplicate identifiers
        control.Identifier += StringHelper.GetRandomAlphabeticString(16);

        // place it at the top-left with an approximately appropriate size
        var size = new Vector2Int(90, 23);
        control.Rectangle = new Rectangle(0, 0, size.X, size.Y);

        // add it to the root node, then queue an invalidation
        DialogViewModel.Dialog.Root.AddChild(control);
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }
}
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RsrcUtilities.Controls;
using RsrcUtilities.Generators.Implementations;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.Layout.Implementations;
using RsrcUtilities.RsrcArchitect.Services;
using RsrcUtilities.RsrcArchitect.ViewModels.Factories;
using RsrcUtilities.RsrcArchitect.ViewModels.Helpers;
using RsrcUtilities.RsrcArchitect.ViewModels.Messages;
using RsrcUtilities.RsrcArchitect.ViewModels.Types;
using RsrcUtilities.Serializers.Implementations;

namespace RsrcUtilities.RsrcArchitect.ViewModels;

public partial class DialogEditorViewModel : ObservableObject, IRecipient<CanvasInvalidationMessage>
{
    public DialogEditorViewModel(Dialog dialog, ICanvasInvalidationService canvasInvalidationService,
        IFilesService filesService, SettingsViewModel settingsViewModel)
    {
        _canvasInvalidationService = canvasInvalidationService;
        _filesService = filesService;
        _settingsViewModel = settingsViewModel;
        Dialog = dialog;
        RebuildControlViewModels();
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    private readonly ICanvasInvalidationService _canvasInvalidationService;
    private readonly IFilesService _filesService;
    private readonly SettingsViewModel _settingsViewModel;

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
            SelectedControlViewModel = value != null ? ControlViewModelFactory.Create(value.Data, s =>
            {
                return Dialog.Root.Flatten().Any(x => x.Identifier.Equals(s, StringComparison.InvariantCultureIgnoreCase));
            }) : null;
            OnPropertyChanged(nameof(SelectedControlViewModel));
        }
    }

    public bool IsNodeSelected => SelectedNode != null;
    private Grips? _currentGrip;
    private Rectangle _gripStartControlRectangle;
    private Vector2 _gripStartPointerPosition;
    private TreeNode<Control>? _selectedNode;
    // private ObservableCollection<ControlViewModel> _controlViewModels = new();

    public ControlViewModel? SelectedControlViewModel { get; private set; }

    // public ObservableCollection<ControlViewModel> ControlViewModels
    // {
    //     get => _controlViewModels;
    //     private set => SetProperty(ref _controlViewModels, value);
    // }

    public Dialog Dialog { get; }

    #region Private Methods

    private void RebuildControlViewModels()
    {
        // ControlViewModels.Clear();
        // foreach (var control in Dialog.Root.Flatten()) ControlViewModels.Add(ControlViewModelFactory.Create(control));
    }

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

    #endregion

    #region Commands

    [RelayCommand]
    private void CreateControl(string tool)
    {
        if (TryCreateControlFromName(out var control, tool))
        {
            control.Identifier = StringHelper.GetRandomAlphabeticString(16);
            control.Rectangle = new Rectangle(10, 10, 90, 25);

            Dialog.Root.AddChild(control);
            RebuildControlViewModels();
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    [RelayCommand]
    private void PointerPress(Vector2 position)
    {
        // TODO: transform canvas-space position to dialog-space position?

        // do grip-test first, because clicks outside of control bounds can grip too
        var isGripHit = false;
        if (SelectedNode != null) isGripHit = GetGrip(SelectedNode.Data, position) != null;

        if (!isGripHit) SelectedNode = GetControlNodeAtPosition(position);

        if (SelectedNode != null)
        {
            _currentGrip = GetGrip(SelectedNode.Data, position);
            _gripStartControlRectangle = SelectedNode.Data.Rectangle;
            _gripStartPointerPosition = ProcessPosition(position);
        }

        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand]
    private void PointerRelease()
    {
        _currentGrip = null;
    }

    [RelayCommand]
    private void PointerMove(Vector2 position)
    {
        position = ProcessPosition(position);

        if (SelectedControlViewModel != null)
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
                    
                    SelectedControlViewModel.Width =  (int)(_gripStartControlRectangle.Width + (position.X - _gripStartPointerPosition.X));
                    SelectedControlViewModel.Height = (int)(_gripStartControlRectangle.Height + (position.Y - _gripStartPointerPosition.Y)); 
                    break;
                case Grips.TopRight:
                    position.X = Math.Max(position.X, _gripStartControlRectangle.X);
                    position.Y = Math.Min(position.Y, _gripStartControlRectangle.Bottom);
                    
                    SelectedControlViewModel.Y = (int)position.Y;
                    SelectedControlViewModel.Width = (int)(_gripStartControlRectangle.Width + (position.X - _gripStartPointerPosition.X));
                    SelectedControlViewModel.Height = (int)(_gripStartControlRectangle.Height + (_gripStartPointerPosition.Y - position.Y));
                    break;
                case Grips.Left:
                    position.X = Math.Min(position.X, _gripStartControlRectangle.Right);
                    
                    SelectedControlViewModel.X = (int)position.X;
                    SelectedControlViewModel.Width = (int)(_gripStartControlRectangle.Width +
                                                           (_gripStartPointerPosition.X - position.X));
                    break;
                case Grips.Right:
                    position.X = Math.Max(position.X, _gripStartControlRectangle.X);
                    SelectedControlViewModel.Width = (int)(_gripStartControlRectangle.Width + (position.X - _gripStartPointerPosition.X));
                    break;
                case Grips.Bottom:
                    position.Y = Math.Max(position.Y, _gripStartControlRectangle.Y);
                    
                    SelectedControlViewModel.Height = (int)(_gripStartControlRectangle.Height + (position.Y - _gripStartPointerPosition.Y));

                    break;
                case Grips.Top:
                    position.Y = Math.Min(position.Y, _gripStartControlRectangle.Bottom);
                    
                    SelectedControlViewModel.Y = (int)(position.Y);
                    SelectedControlViewModel.Height = (int)(_gripStartControlRectangle.Height +
                                                            (_gripStartPointerPosition.Y - position.Y));
                    break;
                case Grips.Move:
                    SelectedControlViewModel.X = (int)(_gripStartControlRectangle.X + (position.X - _gripStartPointerPosition.X));
                    SelectedControlViewModel.Y = (int)(_gripStartControlRectangle.Y + (position.Y - _gripStartPointerPosition.Y));
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
        RebuildControlViewModels();
    }

    [RelayCommand(CanExecute = nameof(IsNodeSelected))]
    private void BringSelectedNodeToFront()
    {
        Dialog.Root.Children.Remove(SelectedNode!);
        Dialog.Root.Children.Add(SelectedNode!);
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        RebuildControlViewModels();
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

        resourceStream.Write(Encoding.Default.GetBytes(serializedDialog));
        await resourceStream.FlushAsync();

        headerStream.Write(Encoding.Default.GetBytes(generatedHeader));
        await headerStream.FlushAsync();
    }

    #endregion

    void IRecipient<CanvasInvalidationMessage>.Receive(CanvasInvalidationMessage message)
    {
        _canvasInvalidationService.Invalidate();
    }
}
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
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

    private TransformationOperation _transformationOperation = TransformationOperation.Empty;

    private List<Rectangle> _transformationStartRectangles = new();
    private Vector2 _transformationStartPointerPosition;
    private Vector2 _panStartPointerPosition;
    private Vector2 _panStartTranslation;
    private float _scale = 1f;
    private bool _isPanning;

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
        SelectedNodes.CollectionChanged += (sender, args) =>
        {
            OnPropertyChanged(nameof(HasSelection));
            DeleteSelectedNodeCommand.NotifyCanExecuteChanged();
            BringSelectedNodeToFrontCommand.NotifyCanExecuteChanged();
            SelectedControlViewModels.Clear();
            foreach (var node in SelectedNodes)
            {
                SelectedControlViewModels.Add(ControlViewModelFactory.Create(node.Data,
                    s =>
                    {
                        return DialogViewModel.Dialog.Root.Flatten().Any(x =>
                            x.Identifier.Equals(s, StringComparison.InvariantCultureIgnoreCase));
                    }));
            }
        };
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public List<ToolboxItemViewModel> ToolboxItemViewModels { get; }
    public DialogViewModel DialogViewModel { get; }
    public string FriendlyName { get; }

    public ObservableCollection<TreeNode<Control>> SelectedNodes { get; } = new();

    public float Scale
    {
        get => _scale;
        set
        {
            SetProperty(ref _scale, Math.Max(0.25f, value));
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
        }
    }

    public bool HasSelection => SelectedControlViewModels.Count > 0;
    public ObservableCollection<ControlViewModel> SelectedControlViewModels { get; } = new();
    public Vector2 Translation { get; private set; } = Vector2.Zero;


    #region Private Methods

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
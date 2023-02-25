using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.Layout.Implementations;
using RsrcUtilities.Layout.Interfaces;
using RsrcUtilities.RsrcArchitect.Services;
using RsrcUtilities.RsrcArchitect.ViewModels.Helpers;

namespace RsrcUtilities.RsrcArchitect.ViewModels;

public partial class DialogEditorViewModel : ObservableObject
{
    private readonly ICanvasInvalidationService _canvasInvalidationService;

    public DialogEditorViewModel(ICanvasInvalidationService canvasInvalidationService)
    {
        _canvasInvalidationService = canvasInvalidationService;
        LayoutEngine = new DefaultLayoutEngine();

        Dialog = new Dialog
        {
            Identifier = "IDD_ABOUTBOX",
            Width = 600,
            Height = 400,
            Root = new TreeNode<Control>(new Panel())
        };
    }

    public ILayoutEngine LayoutEngine { get; }

    public Dialog Dialog { get; }

    public TreeNode<Control>? SelectedNode
    {
        get => _selectedNode;
        private set { SetProperty(ref _selectedNode, value); OnPropertyChanged(nameof(IsNodeSelected)); }
    }

    public bool IsNodeSelected => SelectedNode != null;
    private Grips? _currentGrip;
    private Rectangle _gripStartControlRectangle;
    private Vector2 _gripStartPointerPosition;

    [ObservableProperty] private float _gripDistance = 10f;

    private TreeNode<Control>? _selectedNode;

    private enum Grips
    {
        Move,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
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
                new Vector2(control.Rectangle.X, control.Rectangle.Y)) < GripDistance)
            return Grips.TopLeft;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.Right, control.Rectangle.Y)) < GripDistance)
            return Grips.TopRight;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.X, control.Rectangle.Bottom)) < GripDistance)
            return Grips.BottomLeft;
        if (Vector2.Distance(position,
                new Vector2(control.Rectangle.Right, control.Rectangle.Bottom)) <
            GripDistance)
            return Grips.BottomRight;
        if (control.Rectangle.Contains(new Vector2Int(position)))
            return Grips.Move;
        return null;
    }

    [RelayCommand]
    private void CreateControl(string name)
    {
        if (TryCreateControlFromName(out var control, name))
        {
            control.Identifier = StringHelper.GetRandomAlphabeticString(16);
            control.Rectangle = new Rectangle(10, 10, 90, 25);

            Dialog.Root.AddChild(control);
            _canvasInvalidationService.Invalidate();
        }
    }

    [RelayCommand]
    private void PointerPress(Vector2 position)
    {
        // TODO: transform canvas-space position to dialog-space position?

        // do grip-test first, because clicks outside of control bounds can grip too
        var isGripHit = false;
        if (SelectedNode != null)
        {
            isGripHit = GetGrip(SelectedNode.Data, position) != null;
        }

        if (!isGripHit) SelectedNode = GetControlNodeAtPosition(position);

        if (SelectedNode != null)
        {
            _currentGrip = GetGrip(SelectedNode.Data, position);
            _gripStartControlRectangle = SelectedNode.Data.Rectangle;
            _gripStartPointerPosition = position;
        }

        _canvasInvalidationService.Invalidate();
    }

    [RelayCommand]
    private void PointerRelease()
    {
        _currentGrip = null;
    }

    [RelayCommand]
    private void PointerMove(Vector2 position)
    {
        if (SelectedNode != null)
            switch (_currentGrip)
            {
                case Grips.TopLeft:
                    position.X = Math.Min(position.X, _gripStartControlRectangle.Right);
                    position.Y = Math.Min(position.Y, _gripStartControlRectangle.Bottom);
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithX((int)position.X);
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithY((int)position.Y);
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithWidth(
                        (int)(_gripStartControlRectangle.Width +
                              (_gripStartPointerPosition.X - position.X)));
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithHeight(
                        (int)(_gripStartControlRectangle.Height +
                              (_gripStartPointerPosition.Y - position.Y)));
                    break;
                case Grips.BottomLeft:
                    position.X = Math.Min(position.X, _gripStartControlRectangle.Right);
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithX((int)position.X);
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithWidth(
                        (int)(_gripStartControlRectangle.Width + (_gripStartPointerPosition.X - position.X)));
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithHeight((int)Math.Max(0,
                        _gripStartControlRectangle.Height + (position.Y - _gripStartPointerPosition.Y)));
                    break;
                case Grips.BottomRight:
                    position.X = Math.Max(position.X, _gripStartControlRectangle.X);
                    position.Y = Math.Max(position.Y, _gripStartControlRectangle.Y);
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithWidth(
                        (int)(_gripStartControlRectangle.Width + (position.X - _gripStartPointerPosition.X)));
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithHeight(
                        (int)(_gripStartControlRectangle.Height + (position.Y - _gripStartPointerPosition.Y)));
                    break;
                case Grips.TopRight:
                    position.X = Math.Max(position.X, _gripStartControlRectangle.X);
                    position.Y = Math.Min(position.Y, _gripStartControlRectangle.Bottom);
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithWidth(
                        (int)(_gripStartControlRectangle.Width + (position.X - _gripStartPointerPosition.X)));
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithY((int)position.Y);
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithHeight(
                        (int)(_gripStartControlRectangle.Height + (_gripStartPointerPosition.Y - position.Y)));
                    break;
                case Grips.Move:
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithX(
                        (int)(_gripStartControlRectangle.X + (position.X - _gripStartPointerPosition.X)));
                    SelectedNode.Data.Rectangle = SelectedNode.Data.Rectangle.WithY(
                        (int)(_gripStartControlRectangle.Y + (position.Y - _gripStartPointerPosition.Y)));
                    break;
                case null:
                    break;
            }

        _canvasInvalidationService.Invalidate();
    }

    [RelayCommand(CanExecute = nameof(IsNodeSelected))]
    private void DeleteSelectedNode()
    {
        Dialog.Root.Children.Remove(SelectedNode!);
        SelectedNode = null;
        _canvasInvalidationService.Invalidate();
    }

    private TreeNode<Control>? GetControlNodeAtPosition(Vector2 position)
    {
        // TODO: make special case for groupboxes; the center is clickthrough
        return Dialog.Root.Reverse().FirstOrDefault(node =>
            position.X > node.Data.Rectangle.X && position.Y > node.Data.Rectangle.Y &&
            position.X < node.Data.Rectangle.Right && position.Y < node.Data.Rectangle.Bottom);
    }
}
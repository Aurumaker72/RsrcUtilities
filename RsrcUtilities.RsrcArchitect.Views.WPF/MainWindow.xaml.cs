using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Enums;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.Layout.Implementations;
using RsrcUtilities.Layout.Interfaces;
using RsrcUtilities.Serializers.Implementations;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace RsrcUtilities.RsrcArchitect.Views.WPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        _dialog = new Dialog
        {
            Identifier = "IDD_ABOUTBOX",
            Width = 600,
            Height = 400
        };

        var root = new TreeNode<Control>(new Panel
        {
            Rectangle = new Rectangle(0, 0, 0, 0),
            HorizontalAlignment = HorizontalAlignments.Stretch,
            VerticalAlignment = VerticalAlignments.Stretch
        });
        _dialog.Root = root;
        _layoutEngine = new DefaultLayoutEngine();

        DataContext = this;
    }

    private const float GripDistance = 10f;

    private readonly ILayoutEngine _layoutEngine;
    private readonly Dialog _dialog;

    private string _selectedItem;

    public string SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            if (!value.Equals("Cursor"))
            {
                _selectedControl = null;
                MainCanvasControl.InvalidateVisual();
            }
        }
    }

    private Control? _selectedControl;
    private Grips? _selectedControlGrip;
    private Point _gripStartMousePoint;
    private Rectangle _gripStartSelectedControlRectangle;

    private enum Grips
    {
        Move,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private void MainCanvasControl_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        e.Surface.Canvas.DrawRect(0, 0, _dialog.Width, _dialog.Height, new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(240, 240, 240)
        });

        var flattenedControlDictionary = _layoutEngine.DoLayout(_dialog);

        foreach (var pair in flattenedControlDictionary)
        {
            var rectangle = SKRect.Create(pair.Value.X, pair.Value.Y, pair.Value.Width, pair.Value.Height);

            SKPaint textSkPaint = new()
            {
                TextSize = 11f,
                Typeface = SKTypeface.Default
            };

            SKSize GetTextSize(string text)
            {
                var skRect = SKRect.Empty;
                textSkPaint.MeasureText(text, ref skRect);
                return new SKSize(skRect.Width, skRect.Height);
            }

            SKPoint GetRectangleTextCenter(SKRect rectangle, string text)
            {
                var skSize = GetTextSize(text);
                return new SKPoint(rectangle.Left + rectangle.Width / 2 - skSize.Width / 2,
                    rectangle.Top + rectangle.Height / 2 +
                    skSize.Height / 2); // age-old skia quirk: vertical text positioning is fucked
            }

            if (pair.Key is Button button)
            {
                e.Surface.Canvas.DrawRect(rectangle.InflateCopy(1, 1),
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(173, 173, 173) });
                e.Surface.Canvas.DrawRect(rectangle,
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(225, 225, 225) });
                e.Surface.Canvas.DrawText(button.Caption, GetRectangleTextCenter(rectangle, button.Caption),
                    textSkPaint);
            }
            else if (pair.Key is TextBox textBox)
            {
                e.Surface.Canvas.DrawRect(rectangle.InflateCopy(1, 1),
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(122, 122, 122) });
                e.Surface.Canvas.DrawRect(rectangle,
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(255, 255, 255) });
            }
            else if (pair.Key is GroupBox groupBox)
            {
                e.Surface.Canvas.DrawRect(rectangle,
                    new SKPaint { Style = SKPaintStyle.Stroke, Color = new SKColor(180, 180, 180) });
                e.Surface.Canvas.DrawText(groupBox.Caption,
                    new SKPoint(groupBox.Rectangle.X + 10,
                        groupBox.Rectangle.Y + GetTextSize(groupBox.Caption).Height * 1.5f), textSkPaint);
            }
            else if (pair.Key is Panel)
            {
                ; // panel is a nop for now 
            }
            else
            {
                e.Surface.Canvas.DrawRect(rectangle,
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(255, 0, 255) });
            }
        }

        if (_selectedControl != null)
        {
            var rectangle = SKRect.Create(_selectedControl.Rectangle.X, _selectedControl.Rectangle.Y,
                _selectedControl.Rectangle.Width, _selectedControl.Rectangle.Height);

            e.Surface.Canvas.DrawRect(rectangle,
                new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    PathEffect = SKPathEffect.CreateDash(new[] { 5f, 5f }, 0),
                    StrokeWidth = 2f,
                    Color = new SKColor(0, 0, 255, 255)
                });
            e.Surface.Canvas.DrawPoints(SKPointMode.Points, new SKPoint[]
            {
                new(_selectedControl.Rectangle.X, _selectedControl.Rectangle.Y),
                new(_selectedControl.Rectangle.Right, _selectedControl.Rectangle.Y),
                new(_selectedControl.Rectangle.X, _selectedControl.Rectangle.Bottom),
                new(_selectedControl.Rectangle.Right, _selectedControl.Rectangle.Bottom),
            }, new SKPaint()
            {
                StrokeWidth = GripDistance,
                Color = new SKColor(0, 0, 255, 255),
                StrokeCap = SKStrokeCap.Round,
            });
        }
    }

    private void MainCanvasControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var mousePoint = e.GetPosition((IInputElement)sender);

        if (_selectedControl != null)
        {
            _selectedControlGrip = GetGrip(_selectedControl, mousePoint);
            if (_selectedControlGrip != null)
            {
                _gripStartMousePoint = mousePoint;
                _gripStartSelectedControlRectangle = _selectedControl.Rectangle;
                return;
            }
        }
        
        if (SelectedItem == "Cursor")
        {
            // select top-index control at position
            // the VS dialog editor does this wrong, instead picking the most recently mutated control as the top-level one, which causes unexpected behaviour
            var node = GetControlNodeAtPoint(mousePoint);
            
            _selectedControl = node?.Data;

            if (node != null)
            {
                // bring that control to front by re-adding it at the tail
                _dialog.Root.Children.Remove(node);
                _dialog.Root.Children.Add(node);
            }

            MainCanvasControl.InvalidateVisual();
        }

        

        if (SelectedItem == "Cursor") return;

        var identifier = $"IDC_{GetRandomAlphabeticString(16)}";
        
        Control control = SelectedItem switch
        {
            "Button" => new Button { Identifier = identifier },
            "TextBox" => new TextBox { Identifier = identifier },
            "CheckBox" => new CheckBox { Identifier = identifier },
            "GroupBox" => new GroupBox { Identifier = identifier },
            _ => throw new NotImplementedException()
        };

        control.Rectangle = new Rectangle((int)mousePoint.X, (int)mousePoint.Y, 90, 25);

        _dialog.Root.AddChild(control);
        MainCanvasControl.InvalidateVisual();
        MainCanvasControl.CaptureMouse();
    }

    private TreeNode<Control>? GetControlNodeAtPoint(Point point)
    {
        foreach (var node in _dialog.Root.Reverse())
            if (point.X > node.Data.Rectangle.X && point.Y > node.Data.Rectangle.Y &&
                point.X < node.Data.Rectangle.Right && point.Y < node.Data.Rectangle.Bottom)
            {
                return node;
            }

        return null;
    }

    private Grips? GetGrip(Control control, Point mousePoint)
    {
        if (SKPoint.Distance(mousePoint.ToSKPoint(),
                new SKPoint(control.Rectangle.X, control.Rectangle.Y)) < GripDistance)
            return Grips.TopLeft;
        if (SKPoint.Distance(mousePoint.ToSKPoint(),
                new SKPoint(control.Rectangle.Right, control.Rectangle.Y)) < GripDistance)
            return Grips.TopRight;
        if (SKPoint.Distance(mousePoint.ToSKPoint(),
                new SKPoint(control.Rectangle.X, control.Rectangle.Bottom)) < GripDistance)
            return Grips.BottomLeft;
        if (SKPoint.Distance(mousePoint.ToSKPoint(),
                new SKPoint(control.Rectangle.Right, control.Rectangle.Bottom)) <
            GripDistance)
            return Grips.BottomRight;
        if (SKRect.Create(control.Rectangle.X, control.Rectangle.Y,
                control.Rectangle.Width, control.Rectangle.Height)
            .Contains(mousePoint.ToSKPoint()))
            return Grips.Move;

        return null;
    }

    private void MainCanvasControl_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _selectedControlGrip = null;
        MainCanvasControl.ReleaseMouseCapture();
    }

    private void MainCanvasControl_OnMouseMove(object sender, MouseEventArgs e)
    {
        UpdateCursor();

        if (_selectedControl == null || _selectedControlGrip == null) return;

        var mousePoint = e.GetPosition((IInputElement)sender);

        if (_selectedControlGrip == Grips.TopLeft)
        {
            mousePoint.X = Math.Min(mousePoint.X, _gripStartSelectedControlRectangle.Right);
            mousePoint.Y = Math.Min(mousePoint.Y, _gripStartSelectedControlRectangle.Bottom);

            _selectedControl.Rectangle = _selectedControl.Rectangle.WithX((int)mousePoint.X);
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithY((int)mousePoint.Y);
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithWidth(
                (int)(_gripStartSelectedControlRectangle.Width + (_gripStartMousePoint.X - mousePoint.X)));
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithHeight(
                (int)(_gripStartSelectedControlRectangle.Height + (_gripStartMousePoint.Y - mousePoint.Y)));
        }
        else if (_selectedControlGrip == Grips.BottomLeft)
        {
            mousePoint.X = Math.Min(mousePoint.X, _gripStartSelectedControlRectangle.Right);
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithX((int)mousePoint.X);
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithWidth(
                (int)(_gripStartSelectedControlRectangle.Width + (_gripStartMousePoint.X - mousePoint.X)));
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithHeight((int)Math.Max(0,
                _gripStartSelectedControlRectangle.Height + (mousePoint.Y - _gripStartMousePoint.Y)));
        }
        else if (_selectedControlGrip == Grips.BottomRight)
        {
            mousePoint.X = Math.Max(mousePoint.X, _gripStartSelectedControlRectangle.X);
            mousePoint.Y = Math.Max(mousePoint.Y, _gripStartSelectedControlRectangle.Y);

            _selectedControl.Rectangle = _selectedControl.Rectangle.WithWidth(
                (int)(_gripStartSelectedControlRectangle.Width + (mousePoint.X - _gripStartMousePoint.X)));
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithHeight(
                (int)(_gripStartSelectedControlRectangle.Height + (mousePoint.Y - _gripStartMousePoint.Y)));
        }
        else if (_selectedControlGrip == Grips.TopRight)
        {
            mousePoint.X = Math.Max(mousePoint.X, _gripStartSelectedControlRectangle.X);
            mousePoint.Y = Math.Min(mousePoint.Y, _gripStartSelectedControlRectangle.Bottom);

            _selectedControl.Rectangle = _selectedControl.Rectangle.WithWidth(
                (int)(_gripStartSelectedControlRectangle.Width + (mousePoint.X - _gripStartMousePoint.X)));
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithY((int)mousePoint.Y);
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithHeight(
                (int)(_gripStartSelectedControlRectangle.Height + (_gripStartMousePoint.Y - mousePoint.Y)));
        }
        else if (_selectedControlGrip == Grips.Move)
        {
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithX(
                (int)(_gripStartSelectedControlRectangle.X + (mousePoint.X - _gripStartMousePoint.X)));
            _selectedControl.Rectangle = _selectedControl.Rectangle.WithY(
                (int)(_gripStartSelectedControlRectangle.Y + (mousePoint.Y - _gripStartMousePoint.Y)));
        }

        MainCanvasControl.InvalidateVisual();
    }

    private void UpdateCursor()
    {
        if (_selectedControlGrip == null) Cursor = Cursors.Arrow;
        Cursor = _selectedControlGrip switch
        {
            Grips.Move => Cursors.ScrollAll,
            Grips.TopLeft or Grips.BottomRight => Cursors.SizeNWSE,
            Grips.TopRight or Grips.BottomLeft => Cursors.SizeNESW,
            _ => Cursor
        };
    }

    private void GenerateButton_OnClick(object sender, RoutedEventArgs e)
    {
        var serializer = new DefaultDialogSerializer();
        var generator = new DefaultResourceGenerator();

        try
        {
            File.WriteAllText("resource.h", generator.Generate(_dialog.Root));
            File.WriteAllText("rsrc.rc", serializer.Serialize(_layoutEngine.DoLayout(_dialog), _dialog));
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.ToString());
        }
    }
    
    private static string GetRandomAlphabeticString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }
}
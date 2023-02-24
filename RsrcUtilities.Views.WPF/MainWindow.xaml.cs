using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Enums;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.Layout.Implementations;
using RsrcUtilities.Layout.Interfaces;
using RsrcUtilities.Views.WinUI;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Button = RsrcUtilities.Controls.Button;
using CheckBox = RsrcUtilities.Controls.CheckBox;
using Control = RsrcUtilities.Controls.Control;
using GroupBox = RsrcUtilities.Controls.GroupBox;
using TextBox = RsrcUtilities.Controls.TextBox;

namespace RsrcUtilities.Views.WPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILayoutEngine _layoutEngine;
    private readonly Dialog _dialog;
    
    private bool _isMouseDown;
    private Point _mouseDownPoint;
    private Control _createdControl;

    public MainWindow()
    {
        InitializeComponent();

        _dialog = new Dialog
        {
            Identifier = "IDD_ABOUTBOX",
            Width = 600,
            Height = 400
        };

        var root = new TreeNode<Control>(new GroupBox
        {
            Identifier = "IDC_GROUPBOX",
            Caption = "Look at me!",
            Rectangle = new Rectangle(0, 0, 0, 0),
            Padding = new Vector2Int(10, 10),
            HorizontalAlignment = HorizontalAlignments.Stretch,
            VerticalAlignment = VerticalAlignments.Stretch
        });
        _dialog.Root = root;
        _layoutEngine = new DefaultLayoutEngine();
        
        Main_CanvasControl.InvalidateVisual();
    }

    private void Main_CanvasControl_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
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
                Typeface = SKTypeface.Default,
            };

            SKSize GetTextSize(string text)
            {
                SKRect skRect = SKRect.Empty;
                textSkPaint.MeasureText(text, ref skRect);
                return new SKSize(skRect.Width, skRect.Height);
            }
            SKPoint GetRectangleTextCenter(SKRect rectangle, string text)
            {
                var skSize = GetTextSize(text);
                return new SKPoint(rectangle.Left + rectangle.Width / 2 - skSize.Width / 2,
                    rectangle.Top + rectangle.Height / 2 + skSize.Height / 2); // age-old skia quirk: vertical text positioning is fucked
            }
            
            if (pair.Key is Button button)
            {
                e.Surface.Canvas.DrawRect(rectangle.InflateCopy(1, 1),
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(173, 173, 173) });
                e.Surface.Canvas.DrawRect(rectangle,
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(225, 225, 225) });
                e.Surface.Canvas.DrawText(button.Caption, GetRectangleTextCenter(rectangle, button.Caption), textSkPaint);
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
                e.Surface.Canvas.DrawText(groupBox.Caption, new SKPoint(groupBox.Rectangle.X + 10, groupBox.Rectangle.Y + GetTextSize(groupBox.Caption).Height * 1.5f), textSkPaint);
            }
            else
            {
                e.Surface.Canvas.DrawRect(rectangle,
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(255, 0, 255) });
            }
        }
    }

    private void Main_CanvasControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = true;

        var selectedItem = (ControlSelection_ListView.SelectedItem as ListViewItem).Tag as string;

        _createdControl = selectedItem switch
        {
            "Button" => new Button(),
            "TextBox" => new TextBox(),
            "CheckBox" => new CheckBox(),
            "GroupBox" => new GroupBox(),
            _ => throw new NotImplementedException()
        };

        var mousePoint = e.GetPosition((IInputElement)sender);
        
        _createdControl.Rectangle = _createdControl.Rectangle.WithX((int)mousePoint.X);
        _createdControl.Rectangle = _createdControl.Rectangle.WithY((int)mousePoint.Y);
        
        _mouseDownPoint = mousePoint;
        
        _dialog.Root.AddChild(_createdControl);

        Main_CanvasControl.CaptureMouse();
    }

    private void Main_CanvasControl_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = false;
        Main_CanvasControl.ReleaseMouseCapture();
    }

    private void Main_CanvasControl_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseDown) return;

        var mousePoint = e.GetPosition((IInputElement)sender);

        if (mousePoint.X < _mouseDownPoint.X)
        {
            _createdControl.Rectangle = _createdControl.Rectangle.WithX((int)mousePoint.X);
            _createdControl.Rectangle =
                _createdControl.Rectangle.WithWidth((int)(_mouseDownPoint.X - mousePoint.X));
        }
        else
        {
            _createdControl.Rectangle =
                _createdControl.Rectangle.WithWidth((int)(mousePoint.X - _createdControl.Rectangle.X));

        }
        if (mousePoint.Y < _mouseDownPoint.Y)
        {
            _createdControl.Rectangle = _createdControl.Rectangle.WithY((int)mousePoint.Y);
            _createdControl.Rectangle =
                _createdControl.Rectangle.WithHeight((int)(_mouseDownPoint.Y - mousePoint.Y));
        }
        else
        {
            _createdControl.Rectangle =
                _createdControl.Rectangle.WithHeight((int)(mousePoint.Y - _createdControl.Rectangle.Y));

        }
        
        Main_CanvasControl.InvalidateVisual();
    }
}
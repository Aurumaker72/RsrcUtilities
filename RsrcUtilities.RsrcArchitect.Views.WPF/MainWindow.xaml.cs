using System.Numerics;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CookieCore.Views.WPF.Services;
using RsrcUtilities.Controls;
using RsrcUtilities.RsrcArchitect.Services;
using RsrcUtilities.RsrcArchitect.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Wpf.Ui.Controls.Window;

namespace RsrcUtilities.RsrcArchitect.Views.WPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class MainWindow : FluentWindow, ICanvasInvalidationService
{
    public MainViewModel MainViewModel { get; }

    private static readonly SKPaint SkBlackFontPaint = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        TextAlign = SKTextAlign.Center,
        TextSize = 12
    };

    private static readonly SKFont SkFont = new()
    {
        Edging = SKFontEdging.SubpixelAntialias,
        Size = 12
    };

    public MainWindow()
    {
        InitializeComponent();
        MainViewModel = new MainViewModel(new FilesService(), this);

        DataContext = this;
    }

    void ICanvasInvalidationService.Invalidate()
    {
        SkElement.InvalidateVisual();
    }

    private void SkElement_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        e.Surface.Canvas.DrawRect(0, 0, MainViewModel.DialogEditorViewModel.Dialog.Width,
            MainViewModel.DialogEditorViewModel.Dialog.Height, new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(240, 240, 240)
            });

        var flattenedControlDictionary =
            MainViewModel.DialogEditorViewModel.LayoutEngine.DoLayout(MainViewModel.DialogEditorViewModel.Dialog);

        SKSize GetTextSize(string text)
        {
            var skRect = SKRect.Empty;
            SkBlackFontPaint.MeasureText(text, ref skRect);
            return new SKSize(skRect.Width, skRect.Height);
        }

        foreach (var pair in flattenedControlDictionary)
        {
            var rectangle = SKRect.Create(0, 0, pair.Value.Width, pair.Value.Height);

            e.Surface.Canvas.SetMatrix(SKMatrix.CreateTranslation(pair.Value.X, pair.Value.Y));


            if (pair.Key is Button button)
            {
                e.Surface.Canvas.DrawRect(rectangle.InflateCopy(1, 1),
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(173, 173, 173) });
                e.Surface.Canvas.DrawRect(rectangle,
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(225, 225, 225) });

                e.Surface.Canvas.DrawText(button.Caption,
                    rectangle.MidX,
                    rectangle.MidY + GetTextSize(button.Caption).Height / 2, SkFont, SkBlackFontPaint);
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
                    GetTextSize(groupBox.Caption).Width / 2 + 10f,
                    GetTextSize(groupBox.Caption).Height / 2, SkFont, SkBlackFontPaint);
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

        if (MainViewModel.DialogEditorViewModel.SelectedNode != null)
        {
            var rectangle = SKRect.Create(0, 0,
                MainViewModel.DialogEditorViewModel.SelectedNode.Data.Rectangle.Width,
                MainViewModel.DialogEditorViewModel.SelectedNode.Data.Rectangle.Height);
            e.Surface.Canvas.SetMatrix(SKMatrix.CreateTranslation(
                MainViewModel.DialogEditorViewModel.SelectedNode.Data.Rectangle.X,
                MainViewModel.DialogEditorViewModel.SelectedNode.Data.Rectangle.Y));

            e.Surface.Canvas.DrawRect(rectangle,
                new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = new SKColor(201, 224, 247, 128)
                });

            e.Surface.Canvas.DrawRect(rectangle,
                new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = new SKColor(98, 162, 228)
                });

            e.Surface.Canvas.DrawPoints(SKPointMode.Points, new SKPoint[]
            {
                new(0, 0),
                new(0, rectangle.Height),
                new(rectangle.Width, 0),
                new(rectangle.Width, rectangle.Height)
            }, new SKPaint
            {
                StrokeWidth = MainViewModel.DialogEditorViewModel.GripDistance,
                Color = new SKColor(0, 0, 255, 128),
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            });
        }
    }

    private void SkElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        ((IInputElement)sender).CaptureMouse();

        var position = e.GetPosition((IInputElement)sender);

        MainViewModel.DialogEditorViewModel.PointerPressCommand.Execute(new Vector2((float)position.X,
            (float)position.Y));
    }

    private void SkElement_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        ((IInputElement)sender).ReleaseMouseCapture();
        MainViewModel.DialogEditorViewModel.PointerReleaseCommand.Execute(null);
    }

    private void SkElement_OnMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition((IInputElement)sender);
        MainViewModel.DialogEditorViewModel.PointerMoveCommand.Execute(new Vector2((float)position.X,
            (float)position.Y));
    }

    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete) MainViewModel.DialogEditorViewModel.DeleteSelectedNodeCommand.Execute(null);
    }
}
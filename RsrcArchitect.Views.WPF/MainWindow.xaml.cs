using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using RsrcArchitect.Services;
using RsrcArchitect.ViewModels;
using RsrcArchitect.Views.WPF.Extensions;
using RsrcArchitect.Views.WPF.Services;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using Wpf.Ui.Controls.Window;
using Button = RsrcCore.Controls.Button;
using CheckBox = RsrcCore.Controls.CheckBox;
using GroupBox = RsrcCore.Controls.GroupBox;
using Panel = RsrcCore.Controls.Panel;
using TextBox = RsrcCore.Controls.TextBox;

namespace RsrcArchitect.Views.WPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class MainWindow : FluentWindow, ICanvasInvalidationService
{
    private const float zoomIncrement = 0.5f;

    private static readonly SKPaint SkBlackFontPaint = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        TextAlign = SKTextAlign.Center,
        TextSize = 12
    };

    private static readonly SKPaint SkWhiteFontPaint = new()
    {
        Color = SKColors.White,
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

    private SKElement? _skElement;

    public MainViewModel MainViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();

        MainViewModel = new MainViewModel(new FilesService(), this);
        
        DataContext = this;

        MainViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.SelectedDialogEditorViewModel))
            {
                RefreshControlReferences();
            }
        };
        RefreshControlReferences();
    }

    private void RefreshControlReferences()
    {
        _skElement = TabControl.FindElementByName<SKElement>("SkElement");
        (this as ICanvasInvalidationService).Invalidate();
    }

    void ICanvasInvalidationService.Invalidate()
    {
        _skElement?.InvalidateVisual();
    }


    private void SkElement_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        e.Surface.Canvas.Clear();
        e.Surface.Canvas.SetMatrix(SKMatrix.CreateTranslation(dialogEditorViewModel.Translation.X,
            dialogEditorViewModel.Translation.Y));
        e.Surface.Canvas.Scale(dialogEditorViewModel.Scale);

        var dialogRectangle = SKRect.Create(0, 0, dialogEditorViewModel.DialogViewModel.Width,
            dialogEditorViewModel.DialogViewModel.Height);

        e.Surface.Canvas.DrawRect(dialogRectangle.InflateCopy(1f, 1f), new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0, 120, 215)
        });
        e.Surface.Canvas.DrawRect(dialogRectangle, new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(240, 240, 240)
        });

        e.Surface.Canvas.DrawRect(-1, -30, dialogEditorViewModel.DialogViewModel.Width + 2, 30, new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0, 120, 215)
        });
        e.Surface.Canvas.DrawText(dialogEditorViewModel.DialogViewModel.Caption,
            40f,
            -15 + GetTextSize(dialogEditorViewModel.DialogViewModel.Caption).Height / 2, SkFont, SkWhiteFontPaint);

        var flattenedControlDictionary = dialogEditorViewModel.DialogViewModel.DoLayout();

        SKSize GetTextSize(string text)
        {
            var skRect = SKRect.Empty;
            SkBlackFontPaint.MeasureText(text, ref skRect);
            return new SKSize(skRect.Width, skRect.Height);
        }

        foreach (var pair in flattenedControlDictionary)
        {
            var rectangle = SKRect.Create(0, 0, pair.Value.Width, pair.Value.Height);

            e.Surface.Canvas.Save();
            e.Surface.Canvas.Translate(pair.Value.X, pair.Value.Y);

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
            else if (pair.Key is CheckBox checkBox)
            {
                const float checkSize = 10f;
                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.White
                };
                var checkRectangle = SKRect.Create(0, rectangle.Height / 2 - checkSize / 2, checkSize, checkSize);

                e.Surface.Canvas.DrawRect(checkRectangle, paint);

                paint.Style = SKPaintStyle.Stroke;
                paint.Color = new SKColor(51, 51, 51);
                e.Surface.Canvas.DrawRect(checkRectangle, paint);

                e.Surface.Canvas.DrawText(checkBox.Caption,
                    GetTextSize(checkBox.Caption).Width / 2 + checkSize + 6f,
                    checkRectangle.Top + GetTextSize(checkBox.Caption).Height, SkFont, SkBlackFontPaint);
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

            e.Surface.Canvas.Restore();
        }

        if (dialogEditorViewModel.SelectedControlViewModel != null)
        {
            var rectangle = SKRect.Create(0, 0,
                dialogEditorViewModel.SelectedControlViewModel.Rectangle.Width,
                dialogEditorViewModel.SelectedControlViewModel.Rectangle.Height);
            e.Surface.Canvas.Translate(
                dialogEditorViewModel.SelectedControlViewModel.Rectangle.X,
                dialogEditorViewModel.SelectedControlViewModel.Rectangle.Y);

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
                new(rectangle.Width, rectangle.Height),
                new(rectangle.MidX, 0),
                new(0, rectangle.MidY),
                new(rectangle.Right, rectangle.MidY),
                new(rectangle.MidX, rectangle.Bottom)
            }, new SKPaint
            {
                StrokeWidth = dialogEditorViewModel.DialogEditorSettingsViewModel.GripDistance,
                Color = new SKColor(90, 90, 90),
                IsAntialias = true
            });
        }
    }

    private void SkElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;
        
        ((IInputElement)sender).CaptureMouse();
        var position = e.GetPosition((IInputElement)sender);

        dialogEditorViewModel.PointerPressCommand.Execute(new Vector2(
            (float)position.X,
            (float)position.Y));
    }

    private void SkElement_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;
        ((IInputElement)sender).ReleaseMouseCapture();
        dialogEditorViewModel.PointerReleaseCommand.Execute(null);
    }

    private void SkElement_OnMouseMove(object sender, MouseEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;
        
        var position = e.GetPosition((IInputElement)sender);
        dialogEditorViewModel.PointerMoveCommand.Execute(new Vector2(
            (float)position.X,
            (float)position.Y));
    }

    private void PositioningModeButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;
        
        dialogEditorViewModel.DialogEditorSettingsViewModel.PositioningMode =
            dialogEditorViewModel.DialogEditorSettingsViewModel.PositioningMode.Next();
    }

    private void ZoomOutButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        dialogEditorViewModel.Scale -= zoomIncrement;
    }

    private void ZoomInButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        dialogEditorViewModel.Scale += zoomIncrement;
    }

    private void SkElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
            ZoomInButton_OnClick(sender, null);
        else
            ZoomOutButton_OnClick(sender, null);
    }
}
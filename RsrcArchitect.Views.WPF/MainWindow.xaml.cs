using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using RsrcArchitect.Services;
using RsrcArchitect.ViewModels;
using RsrcArchitect.ViewModels.Types;
using RsrcArchitect.Views.WPF.Services;
using RsrcCore.Controls;
using RsrcCore.Layout.Implementations;
using RsrcCore.Layout.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Window;

namespace RsrcArchitect.Views.WPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class MainWindow : FluentWindow, ICanvasInvalidationService
{
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

    private readonly ILayoutEngine _layoutEngine = new DefaultLayoutEngine();

    public MainWindow()
    {
        InitializeComponent();
        MainViewModel = new MainViewModel(new FilesService(), this);

        DataContext = this;

        MainViewModel.SettingsViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName ==
                nameof(SettingsViewModel.PositioningMode))
                UpdatePositioningModeSymbolIcon();
        };
        UpdatePositioningModeSymbolIcon();
    }

    public MainViewModel MainViewModel { get; }

    void ICanvasInvalidationService.Invalidate()
    {
        SkElement.InvalidateVisual();
    }

    private void UpdatePositioningModeSymbolIcon()
    {
        switch (MainViewModel.SettingsViewModel.PositioningMode)
        {
            case PositioningModes.Arbitrary:
                PositioningModeSymbolIcon.Symbol = SymbolRegular.ArrowMove24;
                break;
            case PositioningModes.Grid:
                PositioningModeSymbolIcon.Symbol = SymbolRegular.Grid24;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SkElement_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        e.Surface.Canvas.Clear();

        e.Surface.Canvas.DrawRect(0, 0, MainViewModel.DialogEditorViewModel.Dialog.Width,
            MainViewModel.DialogEditorViewModel.Dialog.Height, new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(240, 240, 240)
            });

        var flattenedControlDictionary =
            _layoutEngine.DoLayout(MainViewModel.DialogEditorViewModel.Dialog);

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

            e.Surface.Canvas.Save();
            e.Surface.Canvas.ClipRect(rectangle.InflateCopy(5, 5));

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
                e.Surface.Canvas.DrawRect(checkRectangle.InflateCopy(1f, 1f), paint);

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

        if (MainViewModel.DialogEditorViewModel.SelectedControlViewModel != null)
        {
            var rectangle = SKRect.Create(0, 0,
                MainViewModel.DialogEditorViewModel.SelectedControlViewModel.Rectangle.Width,
                MainViewModel.DialogEditorViewModel.SelectedControlViewModel.Rectangle.Height);
            e.Surface.Canvas.SetMatrix(SKMatrix.CreateTranslation(
                MainViewModel.DialogEditorViewModel.SelectedControlViewModel.Rectangle.X,
                MainViewModel.DialogEditorViewModel.SelectedControlViewModel.Rectangle.Y));

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
                StrokeWidth = MainViewModel.SettingsViewModel.GripDistance,
                Color = new SKColor(90, 90, 90),
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

    private void PositioningModeButton_OnClick(object sender, RoutedEventArgs e)
    {
        MainViewModel.SettingsViewModel.PositioningMode =
            MainViewModel.SettingsViewModel.PositioningMode.Next();
    }
}
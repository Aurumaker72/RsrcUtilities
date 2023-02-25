using System.Numerics;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using RsrcUtilities.Controls;
using RsrcUtilities.RsrcArchitect.Services;
using RsrcUtilities.RsrcArchitect.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace RsrcUtilities.RsrcArchitect.Views.WPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class MainWindow : Window, ICanvasInvalidationService
{
    public MainViewModel MainViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        MainViewModel = new MainViewModel(this);

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

        if (MainViewModel.DialogEditorViewModel.SelectedControl != null)
        {
            var rectangle = SKRect.Create(MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.X,
                MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Y,
                MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Width,
                MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Height);

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
                new(MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.X,
                    MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Y),
                new(MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Right,
                    MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Y),
                new(MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.X,
                    MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Bottom),
                new(MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Right,
                    MainViewModel.DialogEditorViewModel.SelectedControl.Rectangle.Bottom)
            }, new SKPaint
            {
                StrokeWidth = MainViewModel.DialogEditorViewModel.GripDistance,
                Color = new SKColor(0, 0, 255, 255),
                StrokeCap = SKStrokeCap.Round
            });
        }
    }

    private void SkElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition((IInputElement)sender);

        MainViewModel.DialogEditorViewModel.PointerPressCommand.Execute(new Vector2((float)position.X,
            (float)position.Y));
    } 
    private void SkElement_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        MainViewModel.DialogEditorViewModel.PointerReleaseCommand.Execute(null);
    }

    private void SkElement_OnMouseMove(object sender, MouseEventArgs e)
    {        
        var position = e.GetPosition((IInputElement)sender);
        MainViewModel.DialogEditorViewModel.PointerMoveCommand.Execute(new Vector2((float)position.X,
            (float)position.Y));
        
        
    }

   
}
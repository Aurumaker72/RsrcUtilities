using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using RsrcArchitect.ViewModels;
using RsrcArchitect.Views.WPF.Win32;
using RsrcCore.Controls;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Rectangle = RsrcCore.Geometry.Structs.Rectangle;

namespace RsrcArchitect.Views.WPF.Renderers.ControlRenderers;

public class NativeObjectRenderer : IObjectRenderer
{
    public NativeObjectRenderer(nint hwnd)
    {
        _hwnd = Unsafe.As<nint, HWND>(ref hwnd);
    }

    private static readonly Color WindowBackgroundColor = Color.FromArgb(255, 240, 240, 240);
        
    private readonly HWND _hwnd;
    private readonly Dictionary<Control, SKBitmap?> _bitmapCache = new();


    private Rectangle _previousDialogRectangle = Rectangle.Zero;
    private float _previousSnapThreshold;

    public unsafe void Render(SKCanvas canvas, Control control, Rectangle visualBounds)
    {
        if (visualBounds.Width <= 0 || visualBounds.Height <= 0) return;


        // if (_bitmapCache.TryGetValue(control, out var value))
        // {
        //     // hit the cache, but we're not sure if we can use this
        //     // TODO: invalidate cache if size or text or any visual property changed 
        //     canvas.DrawBitmap(value, 0, 0);
        //     return;
        // }
        
        // TODO: finish this


        var skRectangle = SKRect.Create(0, 0, visualBounds.Width, visualBounds.Height);
        var windowsRectangle = new RECT(0, 0, visualBounds.Width, visualBounds.Height);

        using var bitmap = new Bitmap(visualBounds.Width, visualBounds.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);

        var hdc = new HDC(graphics.GetHdc());
        
        
        switch (control)
        {
            case Button button:
            {
                var strPtr = Marshal.StringToHGlobalUni("BUTTON;EDIT;COMBOBOX");
                var hTheme = PInvoke.OpenThemeData(_hwnd, new PCWSTR((char*)strPtr));
                PInvoke.DrawThemeBackground(hTheme, hdc,
                    (int)BUTTONPARTS.BP_PUSHBUTTON, (int)PUSHBUTTONSTATES.PBS_NORMAL, &windowsRectangle);
                break;
            }
            case TextBox:
            {
                var strPtr = Marshal.StringToHGlobalUni("EDIT");

                PInvoke.DrawThemeBackground(PInvoke.OpenThemeData(_hwnd, new PCWSTR((char*)strPtr)), hdc,
                    (int)EDITPARTS.EP_EDITBORDER_NOSCROLL, (int)EDITTEXTSTATES.ETS_NORMAL, &windowsRectangle);
                PInvoke.DrawThemeBackground(PInvoke.OpenThemeData(_hwnd, new PCWSTR((char*)strPtr)), hdc,
                    (int)EDITPARTS.EP_CARET, (int)EDITTEXTSTATES.ETS_NORMAL, &windowsRectangle);

                break;
            }
            case GroupBox groupBox:
            {
                var strPtr = Marshal.StringToHGlobalUni("");

                PInvoke.DrawThemeBackground(PInvoke.OpenThemeData(_hwnd, new PCWSTR((char*)strPtr)), hdc,
                    (int)BUTTONPARTS.BP_GROUPBOX, (int)GROUPBOXSTATES.GBS_NORMAL, &windowsRectangle);
                
                break;
            }
            case CheckBox checkBox:
            {
                var strPtr = Marshal.StringToHGlobalUni("");

                PInvoke.DrawThemeBackground(PInvoke.OpenThemeData(_hwnd, new PCWSTR((char*)strPtr)), hdc,
                    (int)BUTTONPARTS.BP_CHECKBOX, (int)CHECKBOXSTATES.CBS_UNCHECKEDNORMAL, &windowsRectangle);
                
                break;
            }
            case ComboBox:
            {
                // winforms VisualStyleElement.cs
                var strPtr = Marshal.StringToHGlobalUni("COMBOBOX");

                PInvoke.DrawThemeBackground(PInvoke.OpenThemeData(_hwnd, new PCWSTR((char*)strPtr)), hdc,
                    4, 3, &windowsRectangle);
                PInvoke.DrawThemeBackground(PInvoke.OpenThemeData(_hwnd, new PCWSTR((char*)strPtr)), hdc,
                    1, 1, &windowsRectangle);
                PInvoke.DrawThemeBackground(PInvoke.OpenThemeData(_hwnd, new PCWSTR((char*)strPtr)), hdc,
                    6, 1, &windowsRectangle);

                break;
            }
            case Label label:
                break;
            default:
                canvas.DrawRect(skRectangle,
                    new SKPaint { Color = new SKColor(255, 0, 255) });
                break;
        }

        graphics.ReleaseHdc();
        _bitmapCache[control] = bitmap.ToSKBitmap();
        canvas.DrawBitmap(_bitmapCache[control], 0, 0);
    }

    public void Render(SKCanvas canvas, DialogViewModel dialogViewModel)
    {
        var skRectangle = SKRect.Create(0, 0, dialogViewModel.Width,
            dialogViewModel.Height);
        canvas.DrawRect(skRectangle.InflateCopy(1f, 1f), new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0, 120, 215)
        });
        canvas.DrawRect(skRectangle, new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor((uint)WindowBackgroundColor.ToArgb())
        });

        canvas.DrawRect(-1, -30, skRectangle.Width + 2, 30, new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0, 120, 215)
        });
    }

    public void RenderDecorations(SKCanvas canvas, DialogEditorViewModel dialogEditorViewModel,
        DialogEditorSettingsViewModel dialogEditorSettingsViewModel)
    {
        // if anything relevant to grid point rendering changes, regenerate them
        // if (_previousDialogRectangle != new Rectangle(0, 0, dialogEditorViewModel.DialogViewModel.Width,
        //         dialogEditorViewModel.DialogViewModel.Height)
        //     ||
        //     // ReSharper disable once CompareOfFloatsByEqualityOperator
        //     dialogEditorSettingsViewModel.SnapThreshold != _previousSnapThreshold)
        //     _gridPoints = null;


        // if (_gridPoints == null)
        // {
        //     // generate and subsequently validate grid points if invalidated
        //     var points = new List<SKPoint>();
        //     for (var x = 0;
        //          x < dialogEditorViewModel.DialogViewModel.Width /
        //          dialogEditorSettingsViewModel.SnapThreshold;
        //          x++)
        //     for (var y = 0;
        //          y < dialogEditorViewModel.DialogViewModel.Height /
        //          dialogEditorSettingsViewModel.SnapThreshold;
        //          y++)
        //         points.Add(new SKPoint(x * dialogEditorSettingsViewModel.SnapThreshold,
        //             y * dialogEditorSettingsViewModel.SnapThreshold));
        //     _gridPoints = points.ToArray();
        // }

        // if (dialogEditorSettingsViewModel.PositioningMode == PositioningModes.Grid)
        //     // draw the grid points, now that we're sure they exist
        //     canvas.DrawPoints(SKPointMode.Points, _gridPoints, SkGridPaint);

        _previousDialogRectangle = new Rectangle(0, 0, dialogEditorViewModel.DialogViewModel.Width,
            dialogEditorViewModel.DialogViewModel.Height);
        _previousSnapThreshold = dialogEditorSettingsViewModel.SnapThreshold;

        if (dialogEditorViewModel.SelectedControlViewModel == null) return;

        // draw selection rectangle
        var rectangle = SKRect.Create(0, 0,
            dialogEditorViewModel.SelectedControlViewModel.Rectangle.Width,
            dialogEditorViewModel.SelectedControlViewModel.Rectangle.Height);

        canvas.Translate(
            dialogEditorViewModel.SelectedControlViewModel.Rectangle.X,
            dialogEditorViewModel.SelectedControlViewModel.Rectangle.Y);

        canvas.DrawRect(rectangle,
            new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(201, 224, 247, 128)
            });

        canvas.DrawRect(rectangle,
            new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(98, 162, 228)
            });

        // draw cached grid points
        canvas.DrawPoints(SKPointMode.Points, new SKPoint[]
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
            StrokeWidth = dialogEditorSettingsViewModel.SnapThreshold,
            Color = new SKColor(90, 90, 90),
            IsAntialias = true
        });
    }
}
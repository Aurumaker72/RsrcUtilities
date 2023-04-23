using System.Collections.Generic;
using RsrcArchitect.ViewModels;
using RsrcArchitect.ViewModels.Types;
using RsrcCore.Controls;
using RsrcCore.Geometry.Structs;
using SkiaSharp;

namespace RsrcArchitect.Views.WPF.Renderers.ControlRenderers;

public class Windows11ObjectRenderer : IObjectRenderer
{
     private static readonly SKPaint SkBlackFontPaint = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("Microsoft Sans Serif"),
        TextSize = 12
    };

    private static readonly SKPaint SkWhiteFontPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("Microsoft Sans Serif"),
        TextSize = 12
    };

    private static readonly SKFont SkFont = new()
    {
        Edging = SKFontEdging.SubpixelAntialias,
        Typeface = SKTypeface.FromFamilyName("Microsoft Sans Serif"),
        Size = 12
    };

    private static readonly SKPaint SkGridPaint = new()
    {
        Color = SKColors.DarkGray,
        StrokeWidth = 2f
    };

    private static SKSize GetTextSize(string text)
    {
        var skRect = SKRect.Empty;
        SkBlackFontPaint.MeasureText(text, ref skRect);
        return new SKSize(skRect.Width, skRect.Height);
    }

    private SKPoint[]? _gridPoints;
    private Rectangle _previousDialogRectangle = Rectangle.Zero;
    private float _previousSnapThreshold;
    private const float CornerRadius = 3f;
    
    public void Render(SKCanvas canvas, Control control, Rectangle visualBounds)
    {
        var skRectangle = new SKRoundRect(SKRect.Create(0, 0, visualBounds.Width, visualBounds.Height), CornerRadius, CornerRadius);

        switch (control)
        {
            case Button button:
                canvas.DrawRoundRect(skRectangle,
                    new SKPaint { Color = new SKColor(253, 253, 253) });
                canvas.DrawRoundRect(skRectangle,
                    new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(208, 208, 208) });
                canvas.DrawText(button.Caption,
                    skRectangle.Rect.MidX - GetTextSize(button.Caption).Width / 2,
                    skRectangle.Rect.MidY + GetTextSize(button.Caption).Height / 2, SkFont, SkBlackFontPaint);
                break;
            case TextBox:
                canvas.DrawRoundRect(skRectangle,
                    new SKPaint { Color = new SKColor(255, 255, 255) });
                canvas.DrawRoundRect(skRectangle,
                    new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(236, 236, 236) });
                canvas.DrawLine(skRectangle.Rect.Left + CornerRadius, skRectangle.Rect.Bottom, skRectangle.Rect.Right - CornerRadius, skRectangle.Rect.Bottom,
                    new SKPaint { Style = SKPaintStyle.StrokeAndFill, Color = new SKColor(131, 131, 131), StrokeWidth = 1});
                break;
            case GroupBox groupBox:
                canvas.DrawRoundRect(skRectangle,
                    new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(220, 220, 220) });
                canvas.DrawText(groupBox.Caption,
                    10f,
                    GetTextSize(groupBox.Caption).Height / 2, SkFont, SkBlackFontPaint);
                break;
            case CheckBox checkBox:
            {
                const float checkSize = 10f;
                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = new SKColor(243, 243, 243),
                    StrokeWidth = 2f,
                };
                var checkRectangle = new SKRoundRect(SKRect.Create(0, skRectangle.Height / 2 - checkSize / 2, checkSize, checkSize), CornerRadius, CornerRadius);
                canvas.DrawRoundRect(checkRectangle, paint);
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = new SKColor(98, 98, 98);
                canvas.DrawRoundRect(checkRectangle, paint);
                canvas.DrawText(checkBox.Caption,
                    checkSize + 5f,
                    checkRectangle.Rect.Top + GetTextSize(checkBox.Caption).Height, SkFont, SkBlackFontPaint);
                break;
            }
            case ComboBox:
                canvas.DrawRoundRect(skRectangle,
                    new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(210, 210, 210) });
                canvas.DrawRoundRect(skRectangle,
                    new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(253, 253, 253) });
                // TODO: combobox arrow rendering
                break;
            case Label label:
                canvas.DrawText(label.Caption,
                    new SKPoint(0, skRectangle.Rect.MidY + GetTextSize(label.Caption).Height / 2),
                    SkBlackFontPaint);
                break;
            default:
                canvas.DrawRect(skRectangle.Rect,
                    new SKPaint { Color = new SKColor(255, 0, 255) });
                break;
        }
    }

    public void Render(SKCanvas canvas, DialogViewModel dialogViewModel)
    {

        const float titleBarHeight = 30f;
        
        var roundedSkRectangle = new SKRoundRect(SKRect.Create(0, -titleBarHeight, dialogViewModel.Width,
            dialogViewModel.Height + titleBarHeight), CornerRadius * 2f, CornerRadius * 2f);

        canvas.Save();
        canvas.ClipRoundRect(roundedSkRectangle);

        
        
        canvas.DrawRoundRect(roundedSkRectangle, new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            Color = new SKColor(0, 120, 212),
        });
        canvas.DrawRoundRect(roundedSkRectangle, new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(240, 240, 240)
        });
        
        canvas.DrawRect(-1, -titleBarHeight, roundedSkRectangle.Width + 2, titleBarHeight, new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0, 120, 212)
        });

        canvas.Restore();
        
        canvas.DrawText(dialogViewModel.Caption,
            5f,
            -15 + GetTextSize(dialogViewModel.Caption).Height / 2, SkFont, SkWhiteFontPaint);
    }

    public void RenderDecorations(SKCanvas canvas, DialogEditorViewModel dialogEditorViewModel, DialogEditorSettingsViewModel dialogEditorSettingsViewModel)
    {
        // if anything relevant to grid point rendering changes, regenerate them
        if (_previousDialogRectangle != new Rectangle(0, 0, dialogEditorViewModel.DialogViewModel.Width,
                dialogEditorViewModel.DialogViewModel.Height)
            ||
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            dialogEditorSettingsViewModel.SnapThreshold != _previousSnapThreshold)
            _gridPoints = null;


        if (_gridPoints == null)
        {
            // generate and subsequently validate grid points if invalidated
            var points = new List<SKPoint>();
            for (var x = 0;
                 x < dialogEditorViewModel.DialogViewModel.Width /
                 dialogEditorSettingsViewModel.SnapThreshold;
                 x++)
            for (var y = 0;
                 y < dialogEditorViewModel.DialogViewModel.Height /
                 dialogEditorSettingsViewModel.SnapThreshold;
                 y++)
                points.Add(new SKPoint(x * dialogEditorSettingsViewModel.SnapThreshold,
                    y * dialogEditorSettingsViewModel.SnapThreshold));
            _gridPoints = points.ToArray();
        }

        if (dialogEditorSettingsViewModel.PositioningMode == PositioningModes.Grid)
            // draw the grid points, now that we're sure they exist
            canvas.DrawPoints(SKPointMode.Points, _gridPoints, SkGridPaint);

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
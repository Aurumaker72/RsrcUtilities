using System;
using System.Collections.Generic;
using RsrcArchitect.ViewModels;
using RsrcArchitect.ViewModels.Types;
using RsrcCore.Controls;
using RsrcCore.Geometry;
using SkiaSharp;

namespace RsrcArchitect.Views.WPF.Rendering;

internal record struct Ninepatch(Rectangle Source, Rectangle Center);

internal record struct VisualStyle(SKImage Image, SKPaint TextPaint, SKPaint InvertedTextPaint, SKFont Font, Ninepatch Titlebar, Ninepatch Background,
    Ninepatch Raised, Ninepatch Edit,
    Ninepatch GroupBox, Rectangle CheckBox);

public class StyledObjectRenderer
{
    private VisualStyle _visualStyle;
    private SKPoint[]? _gridPoints;
    private Rectangle _previousDialogRectangle = Rectangle.Empty;
    private float _previousGridSize;

    public StyledObjectRenderer()
    {
        _visualStyle = new()
        {
            Image = SKImage.FromEncodedData("Assets/windows-10.png"),
            TextPaint = new()
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("MS Shell Dlg 2"),
                TextSize = 14
            },
            InvertedTextPaint = new()
            {
                Color = SKColors.White,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("MS Shell Dlg 2"),
                TextSize = 14
            },
            Font = new()
            {
                Edging = SKFontEdging.SubpixelAntialias,
                Typeface = SKTypeface.FromFamilyName("MS Shell Dlg 2"),
                Size = 14
            },
            Titlebar = new()
            {
                Source = new(49, 22, 9, 9),
                Center = new(53, 26, 1, 1)
            },
            Background = new()
            {
                Source = new(60, 22, 9, 9),
                Center = new(64, 26, 1, 1)
            },
            Raised = new()
            {
                Source = new(1, 1, 11, 9),
                Center = new(6, 5, 1, 1)
            },
            Edit = new()
            {
                Source = new(20, 1, 5, 5),
                Center = new(22, 3, 1, 1)
            },
            GroupBox = new()
            {
                Source = new(36, 5, 3, 3),
                Center = new(37, 6, 1, 1)
            },
            CheckBox = new Rectangle(0, 327, 52, 52)
        };
    }

    private void DrawImageNinePatch(SKCanvas canvas, SKImage image, Rectangle sourceRectangle,
        Rectangle centerRectangle, SKRect destinationRectangle)
    {
        Vector2Int cornerSize = new(Math.Abs(centerRectangle.X - sourceRectangle.X),
            Math.Abs(centerRectangle.Y - sourceRectangle.Y));

        var topLeft = SKRect.Create(sourceRectangle.X, sourceRectangle.Y, cornerSize.X, cornerSize.Y);
        var bottomLeft = SKRect.Create(sourceRectangle.X, sourceRectangle.Bottom - cornerSize.Y, cornerSize.X, cornerSize.Y);
        var topRight = SKRect.Create(sourceRectangle.Right - cornerSize.X, sourceRectangle.Y, cornerSize.X, cornerSize.Y);
        var bottomRight = SKRect.Create(sourceRectangle.Right - cornerSize.X, sourceRectangle.Bottom - cornerSize.Y, cornerSize.X, cornerSize.Y);
        var left = SKRect.Create(sourceRectangle.X, centerRectangle.Y, cornerSize.X, sourceRectangle.Height - cornerSize.Y * 2);
        var right = SKRect.Create(sourceRectangle.Right - cornerSize.X, centerRectangle.Y, cornerSize.X, sourceRectangle.Height - cornerSize.Y * 2);
        var top = SKRect.Create(centerRectangle.X, sourceRectangle.Y, centerRectangle.Width, cornerSize.Y);
        var bottom = SKRect.Create(centerRectangle.X, sourceRectangle.Bottom - cornerSize.Y, centerRectangle.Width, cornerSize.Y);
        var center = SKRect.Create(centerRectangle.X, centerRectangle.Y, centerRectangle.Width, centerRectangle.Height);

        canvas.DrawImage(image, topLeft, SKRect.Create(destinationRectangle.Left, destinationRectangle.Top, cornerSize.X, cornerSize.Y));
        canvas.DrawImage(image, topRight,
            SKRect.Create(destinationRectangle.Right - cornerSize.X, destinationRectangle.Top, cornerSize.X, cornerSize.Y));
        canvas.DrawImage(image, bottomLeft,
            SKRect.Create(destinationRectangle.Left, destinationRectangle.Bottom - cornerSize.Y, cornerSize.X, cornerSize.Y));
        canvas.DrawImage(image, bottomRight,
            SKRect.Create(destinationRectangle.Right - cornerSize.X, destinationRectangle.Bottom - cornerSize.Y, cornerSize.X, cornerSize.Y));
        canvas.DrawImage(image, left,
            SKRect.Create(destinationRectangle.Left, destinationRectangle.Top + cornerSize.Y, cornerSize.X,
                destinationRectangle.Bottom - cornerSize.Y * 2));
        canvas.DrawImage(image, right,
            SKRect.Create(destinationRectangle.Right - cornerSize.X, destinationRectangle.Top + cornerSize.Y, cornerSize.X,
                destinationRectangle.Bottom - cornerSize.Y * 2));
        canvas.DrawImage(image, top,
            SKRect.Create(destinationRectangle.Left + cornerSize.X, destinationRectangle.Top, destinationRectangle.Right - cornerSize.X * 2,
                cornerSize.Y));
        canvas.DrawImage(image, bottom,
            SKRect.Create(destinationRectangle.Left + cornerSize.X, destinationRectangle.Bottom - cornerSize.Y,
                destinationRectangle.Right - cornerSize.X * 2, cornerSize.Y));
        canvas.DrawImage(image, center,
            SKRect.Create(destinationRectangle.Left + cornerSize.X, destinationRectangle.Top + cornerSize.Y,
                destinationRectangle.Right - cornerSize.X * 2, destinationRectangle.Bottom - cornerSize.Y * 2));
    }

    private static SKSize SKGetTextSize(SKPaint paint, string text)
    {
        var skRect = SKRect.Empty;
        paint.MeasureText(text, ref skRect);
        return new SKSize(skRect.Width, skRect.Height);
    }

    public void Render(SKCanvas canvas, Control control, Rectangle visualBounds)
    {
        var skRectangle = SKRect.Create(0, 0, visualBounds.Width, visualBounds.Height);

        switch (control)
        {
            case Button button:
                DrawImageNinePatch(canvas, _visualStyle.Image, _visualStyle.Raised.Source, _visualStyle.Raised.Center,
                    SKRect.Create(0, 0, visualBounds.Width, visualBounds.Height));
                canvas.DrawText(button.Caption,
                    skRectangle.MidX - SKGetTextSize(_visualStyle.TextPaint, button.Caption).Width / 2,
                    skRectangle.MidY + SKGetTextSize(_visualStyle.TextPaint, button.Caption).Height / 2, _visualStyle.Font, _visualStyle.TextPaint);
                break;
            case TextBox textBox:
                DrawImageNinePatch(canvas, _visualStyle.Image, _visualStyle.Edit.Source, _visualStyle.Edit.Center,
                    SKRect.Create(0, 0, visualBounds.Width, visualBounds.Height));
                break;
            case ComboBox comboBox:
                DrawImageNinePatch(canvas, _visualStyle.Image, _visualStyle.Raised.Source, _visualStyle.Raised.Center,
                    SKRect.Create(0, 0, visualBounds.Width, visualBounds.Height));
                break;
            case GroupBox groupBox:
                DrawImageNinePatch(canvas, _visualStyle.Image, _visualStyle.GroupBox.Source, _visualStyle.GroupBox.Center,
                    SKRect.Create(0, 0, visualBounds.Width, visualBounds.Height));
                canvas.DrawText(groupBox.Caption,
                    new SKPoint(10.0f,
                        SKGetTextSize(_visualStyle.TextPaint, groupBox.Caption).Height / 2), _visualStyle.TextPaint);
                break;
            case CheckBox checkBox:
                canvas.DrawImage(_visualStyle.Image,
                    SKRect.Create(_visualStyle.CheckBox.X, _visualStyle.CheckBox.Y, _visualStyle.CheckBox.Width, _visualStyle.CheckBox.Height),
                    SKRect.Create(0, skRectangle.MidY - 6, 13, 13));

                canvas.DrawText(checkBox.Caption,
                    new SKPoint(20.0f,
                        skRectangle.MidY + SKGetTextSize(_visualStyle.TextPaint, checkBox.Caption).Height / 2), _visualStyle.TextPaint);
                break;
            case Label label:
                canvas.DrawText(label.Caption, 0,
                    skRectangle.MidY + SKGetTextSize(_visualStyle.TextPaint, label.Caption).Height / 2, _visualStyle.Font, _visualStyle.TextPaint);
                break;
            default:
                canvas.DrawRect(skRectangle,
                    new SKPaint { Color = new SKColor(255, 0, 255) });
                break;
        }
    }

    public void Render(SKCanvas canvas, DialogViewModel dialogViewModel)
    {
        var skRectangle = SKRect.Create(0, 0, dialogViewModel.Width,
            dialogViewModel.Height);

        // HACK: we translate the canvas instead of the rect, because negative coordinates break DrawImageNinePatch
        // FIXME: negative coordinates break DrawImageNinePatch
        canvas.Translate(0, -30);
        DrawImageNinePatch(canvas, _visualStyle.Image, _visualStyle.Titlebar.Source, _visualStyle.Titlebar.Center,
            SKRect.Create(0, 0, dialogViewModel.Width, 30));
        canvas.DrawText(dialogViewModel.Caption, 5,
            15 + SKGetTextSize(_visualStyle.InvertedTextPaint, dialogViewModel.Caption).Height / 2, _visualStyle.Font,
            _visualStyle.InvertedTextPaint);
        canvas.Translate(0, 30);

        DrawImageNinePatch(canvas, _visualStyle.Image, _visualStyle.Background.Source, _visualStyle.Background.Center,
            SKRect.Create(0, 0, dialogViewModel.Width, dialogViewModel.Height));
    }

    public void RenderDecorations(SKCanvas canvas, DialogEditorViewModel dialogEditorViewModel,
        DialogEditorSettingsViewModel dialogEditorSettingsViewModel)
    {
        // if anything relevant to grid point rendering changes, regenerate them
        if (_previousDialogRectangle != new Rectangle(0, 0, dialogEditorViewModel.DialogViewModel.Width,
                dialogEditorViewModel.DialogViewModel.Height)
            ||
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            dialogEditorSettingsViewModel.GridSize != _previousGridSize)
            _gridPoints = null;


        if (_gridPoints == null)
        {
            // generate and subsequently validate grid points if invalidated
            var points = new List<SKPoint>();
            for (var x = 0;
                 x < dialogEditorViewModel.DialogViewModel.Width /
                 dialogEditorSettingsViewModel.GridSize + 1;
                 x++)
            for (var y = 0;
                 y < dialogEditorViewModel.DialogViewModel.Height /
                 dialogEditorSettingsViewModel.GridSize + 1;
                 y++)
                points.Add(new SKPoint(x * dialogEditorSettingsViewModel.GridSize,
                    y * dialogEditorSettingsViewModel.GridSize));
            _gridPoints = points.ToArray();
        }

        if (dialogEditorSettingsViewModel.Positioning == Positioning.Grid)
            // draw the grid points, now that we're sure they exist
            canvas.DrawPoints(SKPointMode.Points, _gridPoints, _visualStyle.TextPaint);

        _previousDialogRectangle = new Rectangle(0, 0, dialogEditorViewModel.DialogViewModel.Width,
            dialogEditorViewModel.DialogViewModel.Height);
        _previousGridSize = dialogEditorSettingsViewModel.GridSize;

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
            StrokeWidth = dialogEditorSettingsViewModel.GridSize,
            Color = SKColors.Black.WithAlpha(80)
        });
    }
}
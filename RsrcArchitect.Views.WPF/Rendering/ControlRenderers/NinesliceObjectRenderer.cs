using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RsrcArchitect.ViewModels;
using RsrcCore.Controls;
using RsrcCore.Geometry;
using SkiaSharp;

namespace RsrcArchitect.Views.WPF.Rendering.ControlRenderers;

public class NinesliceObjectRenderer : IObjectRenderer
{
    private readonly Dictionary<string, Slices> _slicesMap = new();

    public NinesliceObjectRenderer()
    {
        
        _slicesMap["button"] = new Slices(Rectangle.Empty, Rectangle.Empty, Rectangle.Empty, Rectangle.Empty,
            Rectangle.Empty, Rectangle.Empty, Rectangle.Empty, Rectangle.Empty, Rectangle.Empty);

        File.WriteAllText("test.json",
            JsonSerializer.Serialize(_slicesMap, new JsonSerializerOptions { WriteIndented = true }));
    }


    public void Render(SKCanvas canvas, Control control, Rectangle visualBounds)
    {
    }

    public void Render(SKCanvas canvas, DialogViewModel dialogViewModel)
    {
    }

    public void RenderDecorations(SKCanvas canvas, DialogEditorViewModel dialogEditorViewModel,
        DialogEditorSettingsViewModel dialogEditorSettingsViewModel)
    {
    }
}
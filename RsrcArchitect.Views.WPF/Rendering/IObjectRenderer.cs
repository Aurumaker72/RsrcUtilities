using RsrcArchitect.ViewModels;
using RsrcCore.Controls;
using RsrcCore.Geometry;
using SkiaSharp;

namespace RsrcArchitect.Views.WPF.Rendering;

public interface IObjectRenderer
{
    void Render(SKCanvas canvas, Control control, Rectangle visualBounds);
    void Render(SKCanvas canvas, DialogViewModel dialogViewModel);
    void RenderDecorations(SKCanvas canvas, DialogEditorViewModel dialogEditorViewModel, DialogEditorSettingsViewModel dialogEditorSettingsViewModel);
}
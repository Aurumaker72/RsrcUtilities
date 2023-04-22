using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using RsrcArchitect.ViewModels;
using RsrcArchitect.Views.WPF.Renderer.ControlRenderers;
using RsrcCore.Controls;
using SkiaSharp;

namespace RsrcArchitect.Views.WPF.Renderer;

public class DialogRenderer
{
    public IObjectRenderer ObjectRenderer { get; set; } = new Windows10ObjectRenderer();
    
    public void Render(DialogEditorViewModel dialogEditorViewModel, SKCanvas canvas)
    {
        // create view matrix from dialog editor data
        canvas.SetMatrix(SKMatrix.CreateScaleTranslation(dialogEditorViewModel.Scale, dialogEditorViewModel.Scale,
            dialogEditorViewModel.Translation.X, dialogEditorViewModel.Translation.Y));

        // draw non-client area and background
        var dialogRectangle = SKRect.Create(0, 0, dialogEditorViewModel.DialogViewModel.Width,
            dialogEditorViewModel.DialogViewModel.Height);

        // do layout pass and get the untransformed dialog-space rectangles
        var controlRectangles = dialogEditorViewModel.DialogViewModel.DoLayout();
        
        canvas.Clear();

        ObjectRenderer.Render(canvas, dialogEditorViewModel.DialogViewModel);
        
        foreach (var (control, rectangle) in controlRectangles)
        {
            canvas.Save();
            canvas.Translate(rectangle.X, rectangle.Y);
            
            ObjectRenderer.Render(canvas, control, rectangle);
            
            canvas.Restore();
        }
        
        ObjectRenderer.RenderDecorations(canvas, dialogEditorViewModel);
    }
    
}
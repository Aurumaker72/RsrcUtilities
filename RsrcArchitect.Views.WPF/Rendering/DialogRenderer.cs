using RsrcArchitect.ViewModels;
using RsrcArchitect.Views.WPF.Rendering.ControlRenderers;
using SkiaSharp;

namespace RsrcArchitect.Views.WPF.Rendering;

public class DialogRenderer
{
    public IObjectRenderer ObjectRenderer { get; set; } = new Windows10ObjectRenderer();
    
    public void Render(DialogEditorSettingsViewModel dialogEditorSettingsViewModel, DialogEditorViewModel dialogEditorViewModel, SKCanvas canvas)
    {
        // create view matrix from dialog editor data
        canvas.SetMatrix(SKMatrix.CreateScaleTranslation(dialogEditorViewModel.Scale, dialogEditorViewModel.Scale,
            dialogEditorViewModel.Translation.X, dialogEditorViewModel.Translation.Y));
        
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
        
        ObjectRenderer.RenderDecorations(canvas, dialogEditorViewModel, dialogEditorSettingsViewModel);
    }
    
}
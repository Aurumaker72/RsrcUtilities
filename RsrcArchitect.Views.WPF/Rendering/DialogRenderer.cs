using RsrcArchitect.ViewModels;
using SkiaSharp;

namespace RsrcArchitect.Views.WPF.Rendering;

public class DialogRenderer
{
    private static StyledObjectRenderer _styledObjectRenderer = new();
    
    
    public void Render(DialogEditorSettingsViewModel dialogEditorSettingsViewModel, DialogEditorViewModel dialogEditorViewModel, SKCanvas canvas)
    {
        // create view matrix from dialog editor data
        canvas.SetMatrix(SKMatrix.CreateScaleTranslation(dialogEditorViewModel.Scale, dialogEditorViewModel.Scale,
            dialogEditorViewModel.Translation.X, dialogEditorViewModel.Translation.Y));
        
        // do layout pass and get the untransformed dialog-space rectangles
        var controlRectangles = dialogEditorViewModel.DialogViewModel.DoLayout();
        
        canvas.Clear();

        _styledObjectRenderer.Render(canvas, dialogEditorViewModel.DialogViewModel);
        
        foreach (var (control, rectangle) in controlRectangles)
        {
            canvas.Save();
            canvas.Translate(rectangle.X, rectangle.Y);
            
            _styledObjectRenderer.Render(canvas, control, rectangle);
            
            canvas.Restore();
        }
        
        _styledObjectRenderer.RenderDecorations(canvas, dialogEditorViewModel, dialogEditorSettingsViewModel);
    }
    
}
using CommunityToolkit.Mvvm.ComponentModel;
using RsrcUtilities.RsrcArchitect.Services;

namespace RsrcUtilities.RsrcArchitect.ViewModels;

public class MainViewModel : ObservableObject
{
    private DialogEditorViewModel _dialogEditorViewModel;

    public DialogEditorViewModel DialogEditorViewModel
    {
        get => _dialogEditorViewModel;
        internal set => SetProperty(ref _dialogEditorViewModel, value);
    }

    public MainViewModel(ICanvasInvalidationService canvasInvalidationService)
    {
        DialogEditorViewModel = new(canvasInvalidationService);
    }
}
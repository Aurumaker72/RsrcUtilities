using CommunityToolkit.Mvvm.ComponentModel;
using RsrcArchitect.Services;
using RsrcCore;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly IFilesService _filesService;
    private DialogEditorViewModel _dialogEditorViewModel;
    private SettingsViewModel _settingsViewModel;

    public MainViewModel(IFilesService filesService, ICanvasInvalidationService canvasInvalidationService)
    {
        _filesService = filesService;
        SettingsViewModel = new SettingsViewModel();
        DialogEditorViewModel = new DialogEditorViewModel(new Dialog
        {
            Identifier = "IDD_ABOUTBOX",
            Width = 600,
            Height = 400,
            Root = new TreeNode<Control>(new Panel())
        }, canvasInvalidationService, filesService, SettingsViewModel);
    }

    public DialogEditorViewModel DialogEditorViewModel
    {
        get => _dialogEditorViewModel;
        internal set => SetProperty(ref _dialogEditorViewModel, value);
    }

    public SettingsViewModel SettingsViewModel
    {
        get => _settingsViewModel;
        internal set => SetProperty(ref _settingsViewModel, value);
    }
}
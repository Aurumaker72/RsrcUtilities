using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RsrcUtilities.Controls;
using RsrcUtilities.Generators.Implementations;
using RsrcUtilities.Layout.Implementations;
using RsrcUtilities.RsrcArchitect.Services;
using RsrcUtilities.Serializers.Implementations;

namespace RsrcUtilities.RsrcArchitect.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IFilesService _filesService;
    private DialogEditorViewModel _dialogEditorViewModel;

    public DialogEditorViewModel DialogEditorViewModel
    {
        get => _dialogEditorViewModel;
        internal set => SetProperty(ref _dialogEditorViewModel, value);
    }
    private SettingsViewModel _settingsViewModel;

    public SettingsViewModel SettingsViewModel
    {
        get => _settingsViewModel;
        internal set => SetProperty(ref _settingsViewModel, value);
    }
    
    public MainViewModel(IFilesService filesService, ICanvasInvalidationService canvasInvalidationService)
    {
        _filesService = filesService;
        SettingsViewModel = new();
        DialogEditorViewModel = new DialogEditorViewModel(new Dialog
        {
            Identifier = "IDD_ABOUTBOX",
            Width = 600,
            Height = 400,
            Root = new TreeNode<Control>(new Panel())
        }, canvasInvalidationService, filesService, SettingsViewModel);
    }

    
}
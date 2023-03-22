using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.Services;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels;

public partial class MainViewModel : ObservableObject, IRecipient<CanvasInvalidationMessage>
{
    private readonly IFilesService _filesService;
    private readonly ICanvasInvalidationService _canvasInvalidationService;

    public SettingsViewModel SettingsViewModel { get; }

    public ObservableCollection<DialogEditorViewModel> DialogEditorViewModels { get; } = new();

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsDialogEditorViewModelSelected))]
    private DialogEditorViewModel? _selectedDialogEditorViewModel;

    public bool IsDialogEditorViewModelSelected => _selectedDialogEditorViewModel != null;

    public MainViewModel(IFilesService filesService, ICanvasInvalidationService canvasInvalidationService)
    {
        _filesService = filesService;
        _canvasInvalidationService = canvasInvalidationService;
        SettingsViewModel = new SettingsViewModel();
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    partial void OnSelectedDialogEditorViewModelChanged(DialogEditorViewModel? value)
    {
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }
    
    [RelayCommand]
    private void CreateDialogEditor()
    {
        DialogEditorViewModels.Add(new DialogEditorViewModel(new Dialog
        {
            Identifier = "IDD_ABOUTBOX",
            Width = 600,
            Height = 400,
            Root = new TreeNode<Control>(new Panel())
        }, _filesService, SettingsViewModel, "New Dialog Project"));
    }
    
    void IRecipient<CanvasInvalidationMessage>.Receive(CanvasInvalidationMessage message)
    {
        _canvasInvalidationService.Invalidate();
    }
}
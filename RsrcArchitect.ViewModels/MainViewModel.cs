using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.Services;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels;

public partial class MainViewModel : ObservableObject, IRecipient<DialogEditorViewModelClosingMessage>
{
    private readonly IFilePickerService _filePickerService;

    public ObservableCollection<DialogEditorViewModel> DialogEditorViewModels { get; } = new();
    public DialogEditorSettingsViewModel DialogEditorSettingsViewModel { get; } = new();

    [ObservableProperty] private DialogEditorViewModel? _selectedDialogEditorViewModel = null;

    public MainViewModel(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    [RelayCommand]
    private void CreateProject()
    {
        DialogEditorViewModels.Add(new DialogEditorViewModel(new Dialog
        {
            Identifier = "IDD_ABOUTBOX",
            Width = 600,
            Height = 400,
            Root = new TreeNode<Control>(new Panel())
        }, "New Dialog Project", DialogEditorSettingsViewModel, _filePickerService));
    }

    void IRecipient<DialogEditorViewModelClosingMessage>.Receive(DialogEditorViewModelClosingMessage message)
    {
        DialogEditorViewModels.Remove(message.Value);
    }
}
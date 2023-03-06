using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public MainViewModel(IFilesService filesService, ICanvasInvalidationService canvasInvalidationService)
    {
        _filesService = filesService;
        DialogEditorViewModel = new DialogEditorViewModel(canvasInvalidationService);
    }

    [RelayCommand]
    private async Task Save()
    {
        var serializedDialog = new RcDialogSerializer().Serialize(
            new DefaultLayoutEngine().DoLayout(DialogEditorViewModel.Dialog), DialogEditorViewModel.Dialog);
        var generatedHeader = new CxxHeaderResourceGenerator().Generate(DialogEditorViewModel.Dialog.Root.Flatten());

        var resourceFile = await _filesService.TryPickSaveFileAsync("rsrc_snippet.rc", ("Resource File", new[] { "rc" }));
        var headerFile = await _filesService.TryPickSaveFileAsync("resource.h", ("C/C++ Header File", new[] { "h" }));

        if (resourceFile == null || headerFile == null)
        {
            return;
        }
        
        await using var resourceStream = await resourceFile.OpenStreamForWriteAsync();
        await using var headerStream = await headerFile.OpenStreamForWriteAsync();

        resourceStream.Write(Encoding.Default.GetBytes(serializedDialog));
        await resourceStream.FlushAsync();
        
        headerStream.Write(Encoding.Default.GetBytes(generatedHeader));
        await headerStream.FlushAsync();
    }
}
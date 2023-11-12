using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.Services;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore;
using RsrcCore.Controls;
using RsrcCore.Serializers.Implementations;

namespace RsrcArchitect.ViewModels;

public partial class MainViewModel : ObservableObject, IRecipient<DialogEditorViewModelClosingMessage>
{
    private readonly IFilePickerService _filePickerService;
    private readonly IInputService _inputService;
    
    public ObservableCollection<DialogEditorViewModel> DialogEditorViewModels { get; } = new();
    public DialogEditorSettingsViewModel DialogEditorSettingsViewModel { get; } = new();

    [ObservableProperty] private DialogEditorViewModel? _selectedDialogEditorViewModel = null;

    public MainViewModel(IFilePickerService filePickerService, IInputService inputService)
    {
        _filePickerService = filePickerService;
        _inputService = inputService;
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
        }, "New Dialog Project", DialogEditorSettingsViewModel, _filePickerService, _inputService));
    }

    [RelayCommand]
    private void Open()
    {
        var dialog = new RcDialogSerializer().Deserialize("""
                                                          IDD_MOVIE_PLAYBACK_DIALOG DIALOGEX 0, 0, 342, 342
                                                          STYLE DS_SETFONT | DS_MODALFRAME | DS_FIXEDSYS | DS_CENTER | WS_POPUP | WS_CAPTION | WS_SYSMENU
                                                          CAPTION "Play Movie"
                                                          FONT 8, "MS Shell Dlg", 400, 0, 0x0
                                                          BEGIN
                                                              EDITTEXT        IDC_INI_MOVIEFILE,59,20,212,12,ES_AUTOHSCROLL
                                                              PUSHBUTTON      "Browse...",IDC_MOVIE_BROWSE,281,19,50,14
                                                              PUSHBUTTON      "Refresh",IDC_MOVIE_REFRESH,281,3,50,14
                                                              CONTROL         "Open Read-Only",IDC_MOVIE_READONLY,"Button",BS_AUTOCHECKBOX | WS_TABSTOP,203,6,69,10
                                                              EDITTEXT        IDC_PAUSEAT_FIELD,80,320,44,13,ES_AUTOHSCROLL | ES_NUMBER
                                                              DEFPUSHBUTTON   "OK",IDC_OK,225,320,50,14
                                                              PUSHBUTTON      "Cancel",IDC_CANCEL,281,320,50,14
                                                              GROUPBOX        "Movie Settings",IDC_ROM_HEADER_INFO_TEXT,9,118,153,195
                                                              LTEXT           "Name:",IDC_ROM_INTERNAL_NAME_TEXT,23,139,31,8
                                                              LTEXT           "Country:",IDC_ROM_COUNTRY_TEXT,23,151,32,8
                                                              EDITTEXT        IDC_ROM_INTERNAL_NAME,56,139,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_ROM_COUNTRY,56,151,88,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              GROUPBOX        "Plugins",IDC_ROM_PLUGINS,15,179,140,62
                                                              LTEXT           "Video:",IDC_GFXPLUGIN,23,191,32,10
                                                              LTEXT           "Sound:",IDC_SOUNDPLUGIN,23,203,31,8
                                                              LTEXT           "Input:",IDC_INPUTPLUGIN,23,215,31,8
                                                              LTEXT           "RSP:",IDC_RSPPLUGIN,23,227,32,8
                                                              LTEXT           "Pause at frame:",IDC_PAUSEAT_LABEL,23,323,52,10
                                                              LTEXT           "Movie File:",IDC_INI_MOVIEFILE_TEXT,16,22,40,8
                                                              EDITTEXT        IDC_INI_AUTHOR,59,42,272,12,ES_AUTOHSCROLL
                                                              EDITTEXT        IDC_INI_DESCRIPTION,59,60,272,24,ES_MULTILINE | ES_AUTOVSCROLL | ES_WANTRETURN | WS_VSCROLL
                                                              LTEXT           "CRC:",IDC_ROM_CRC_TEXT,23,163,31,8
                                                              EDITTEXT        IDC_ROM_CRC,56,163,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              GROUPBOX        "ROM",IDC_ROM_INFO_TEXT,16,127,139,50
                                                              LTEXT           "Author:",IDC_MOVIE_AUTHOR_TEXT,25,44,29,8
                                                              LTEXT           "Description:",IDC_MOVIE_DESCRIPTION_TEXT,12,68,42,8
                                                              EDITTEXT        IDC_MOVIE_VIDEO_TEXT,56,191,98,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_SOUND_TEXT,56,203,98,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_INPUT_TEXT,56,215,98,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_RSP_TEXT,56,227,98,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              GROUPBOX        "Controllers",IDC_MOVIE_CONTROLLERS,15,245,140,62
                                                              LTEXT           "1:",IDC_CONTROLLER1,23,257,32,10
                                                              LTEXT           "2:",IDC_CONTROLLER2,23,269,31,8
                                                              LTEXT           "3:",IDC_CONTROLLER3,23,281,31,8
                                                              LTEXT           "4:",IDC_CONTROLLER4,23,293,32,8
                                                              EDITTEXT        IDC_MOVIE_CONTROLLER1_TEXT,55,257,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_CONTROLLER2_TEXT,55,269,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_CONTROLLER3_TEXT,55,281,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_CONTROLLER4_TEXT,55,293,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              LTEXT           "1:",IDC_CONTROLLER5,193,257,32,10
                                                              LTEXT           "2:",IDC_CONTROLLER6,193,269,31,8
                                                              LTEXT           "3:",IDC_CONTROLLER7,193,281,31,8
                                                              LTEXT           "4:",IDC_CONTROLLER8,193,293,32,8
                                                              EDITTEXT        IDC_MOVIE_CONTROLLER1_TEXT2,225,257,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_CONTROLLER2_TEXT2,225,269,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_CONTROLLER3_TEXT2,225,281,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_CONTROLLER4_TEXT2,225,293,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              LTEXT           "Length:",IDC_MOVIE_LENGTH_TEXT,25,91,32,8
                                                              LTEXT           "Frames:",IDC_MOVIE_FRAMES_TEXT,24,105,32,8
                                                              LTEXT           "Re-records:",IDC_MOVIE_RERECORDS_TEXT,186,91,43,8
                                                              EDITTEXT        IDC_MOVIE_LENGTH,58,91,104,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_FRAMES,58,105,104,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_RERECORDS,232,91,93,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              LTEXT           "Starts From:",IDC_STARTSFROM_LABEL,184,105,45,8
                                                              EDITTEXT        IDC_FROMSNAPSHOT_TEXT,232,105,56,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              GROUPBOX        "Your Settings",IDC_ROM_HEADER_INFO_TEXT2,179,118,153,195
                                                              LTEXT           "Country:",IDC_ROM_COUNTRY_TEXT2,193,151,32,8
                                                              EDITTEXT        IDC_ROM_INTERNAL_NAME2,226,139,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_ROM_COUNTRY2,226,151,88,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              GROUPBOX        "Plugins",IDC_ROM_PLUGINS2,185,179,140,62
                                                              LTEXT           "Video:",IDC_GFXPLUGIN2,193,191,32,10
                                                              LTEXT           "Sound:",IDC_SOUNDPLUGIN2,193,203,31,8
                                                              LTEXT           "Input:",IDC_INPUTPLUGIN2,193,215,31,8
                                                              LTEXT           "RSP:",IDC_RSPPLUGIN2,193,227,32,8
                                                              LTEXT           "CRC:",IDC_ROM_CRC_TEXT2,193,163,31,8
                                                              EDITTEXT        IDC_ROM_CRC3,226,163,91,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              GROUPBOX        "ROM",IDC_ROM_INFO_TEXT2,186,127,139,50
                                                              EDITTEXT        IDC_MOVIE_VIDEO_TEXT2,226,191,98,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_SOUND_TEXT2,226,203,98,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_INPUT_TEXT2,226,215,98,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              EDITTEXT        IDC_MOVIE_RSP_TEXT2,226,227,98,12,ES_AUTOHSCROLL | ES_READONLY | NOT WS_BORDER
                                                              GROUPBOX        "Controllers",IDC_MOVIE_CONTROLLERS2,185,245,140,62
                                                              LTEXT           "Name:",IDC_ROM_INTERNAL_NAME_TEXT2,193,139,31,8
                                                          END

                                                          """);
        DialogEditorViewModels.Add(new DialogEditorViewModel(dialog, "New Dialog Project", DialogEditorSettingsViewModel, _filePickerService, _inputService));
        // var result = await _filePickerService.TryPickOpenFileAsync(new[] { ".rc" });
        //
        // if (result == null)
        // {
        //     return;
        // }
        //
        // var text = File.ReadAllText(result);
        // var dialog = new RcDialogSerializer().Deserialize(text);
        // DialogEditorViewModels.Add(new DialogEditorViewModel(dialog, "New Dialog Project", DialogEditorSettingsViewModel, _filePickerService));
    }
    
    void IRecipient<DialogEditorViewModelClosingMessage>.Receive(DialogEditorViewModelClosingMessage message)
    {
        DialogEditorViewModels.Remove(message.Value);
    }
}
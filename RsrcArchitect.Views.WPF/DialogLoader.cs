using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using RsrcArchitect.ViewModels;
using RsrcArchitect.Views.WPF.Extensions;
using RsrcCore.Controls;
using RsrcCore.Geometry;

public static class DialogLoader
{
    private const string ClassName = "previewWindow";
    private static HINSTANCE _instance;


    private static unsafe void CreateControl(HWND dialogHwnd, Control control, Rectangle rectangle)
    {
        if (control is Button button)
        {
            PInvoke.CreateWindowEx(0L, "BUTTON".ToPcwstr(), button.Caption.ToPcwstr(),
                WINDOW_STYLE.WS_BORDER | WINDOW_STYLE.WS_TABSTOP | WINDOW_STYLE.WS_CHILD | (WINDOW_STYLE)0x00000001L, rectangle.X, rectangle.Y,
                rectangle.Width, rectangle.Height, dialogHwnd, HMENU.Null,
                _instance);
        }
    }

    public static unsafe void ShowDialogFromRcString(DialogViewModel dialogViewModel)
    {
        var wc = new WNDCLASSEXW();
        MSG msg;
        HWND hwnd;

        _instance = PInvoke.GetModuleHandle((PCWSTR)null);

        wc.cbSize = 80U;
        wc.lpfnWndProc = PInvoke.DefWindowProc;
        wc.hInstance = _instance;
        wc.lpszClassName = ClassName.ToPcwstr();

        PInvoke.RegisterClassEx(in wc);

        hwnd = PInvoke.CreateWindowEx(0,
            ClassName.ToPcwstr(),
            dialogViewModel.Caption.ToPcwstr(),
            (WINDOW_STYLE)(0x00000000L | 0x00C00000L | 0x00080000L | 0x00040000L | 0x00020000L | 0x00010000L),
            unchecked((int)0x80000000),
            unchecked((int)0x80000000),
            dialogViewModel.Width,
            dialogViewModel.Height,
            new HWND(0),
            HMENU.Null,
            _instance);

        PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOW);
        PInvoke.UpdateWindow(hwnd);

        foreach (var (key, value) in dialogViewModel.DoLayout())
        {
            CreateControl(hwnd, key, value);
        }

        while (PInvoke.GetMessage(&msg, hwnd, 0, 0))
        {
            PInvoke.TranslateMessage(&msg);
            PInvoke.DispatchMessage(&msg);
        }

        PInvoke.CloseWindow(hwnd);
        PInvoke.DestroyWindow(hwnd);
    }
}
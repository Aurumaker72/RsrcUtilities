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

    // ReSharper disable InconsistentNaming
    // ReSharper disable IdentifierTypo
    private const long ES_AUTOHSCROLL = 0x0080L;
    private const long ES_READONLY = 0x0800L;
    private const long BS_AUTOCHECKBOX = 0x00000003L;
    private const long BS_GROUPBOX = 0x00000007L;
    private const long CBS_DROPDOWN = 0x0002L;
    private const long CBS_DROPDOWNLIST = 0x0003L;
    // ReSharper restore InconsistentNaming
    // ReSharper restore IdentifierTypo

    private static unsafe void CreateControl(HWND dialogHwnd, Control control, Rectangle rectangle)
    {
        WINDOW_STYLE style = WINDOW_STYLE.WS_VISIBLE | WINDOW_STYLE.WS_CHILD;
        if (!control.IsEnabled)
        {
            style |= WINDOW_STYLE.WS_DISABLED;
        }
        
        if (control is Button button)
        {
            PInvoke.CreateWindowEx(0, "BUTTON".ToPcwstr(), button.Caption.ToPcwstr(),
                style, rectangle.X, rectangle.Y, rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is TextBox textBox)
        {
            if (!textBox.IsWriteable) style |= (WINDOW_STYLE)ES_READONLY;
            if (textBox.AllowHorizontalScroll) style |= (WINDOW_STYLE)ES_AUTOHSCROLL;

            PInvoke.CreateWindowEx(WINDOW_EX_STYLE.WS_EX_CLIENTEDGE, "EDIT".ToPcwstr(), "".ToPcwstr(),
                style, rectangle.X, rectangle.Y, rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is CheckBox checkBox)
        {
            PInvoke.CreateWindowEx(0, "BUTTON".ToPcwstr(), checkBox.Caption.ToPcwstr(),
                style | (WINDOW_STYLE)BS_AUTOCHECKBOX, rectangle.X,
                rectangle.Y, rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is GroupBox groupBox)
        {
            PInvoke.CreateWindowEx(0, "BUTTON".ToPcwstr(), groupBox.Caption.ToPcwstr(),
                style | (WINDOW_STYLE)BS_GROUPBOX, rectangle.X, rectangle.Y,
                rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is ComboBox comboBox)
        {
            PInvoke.CreateWindowEx(0, "ComboBox".ToPcwstr(), "?".ToPcwstr(),
                style | (WINDOW_STYLE)CBS_DROPDOWN | (WINDOW_STYLE)CBS_DROPDOWNLIST, rectangle.X, rectangle.Y,
                rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is Label label)
        {
            PInvoke.CreateWindowEx(0, "STATIC".ToPcwstr(), label.Caption.ToPcwstr(),
                style, rectangle.X, rectangle.Y, rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
    }

    public static unsafe void ShowDialogFromRcString(DialogViewModel dialogViewModel)
    {
        _instance = PInvoke.GetModuleHandle((PCWSTR)null);

        var wc = new WNDCLASSW
        {
            lpfnWndProc = PInvoke.DefWindowProc,
            hInstance = _instance,
            lpszClassName = ClassName.ToPcwstr(),
            style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW
        };
        PInvoke.RegisterClass(in wc);
        
        var hwnd = PInvoke.CreateWindowEx(WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW,
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

        foreach (var (key, value) in dialogViewModel.DoLayout()) CreateControl(hwnd, key, value);

        MSG msg;
        while (PInvoke.GetMessage(&msg, hwnd, 0, 0))
        {
            PInvoke.TranslateMessage(&msg);
            PInvoke.DispatchMessage(&msg);
        }

        PInvoke.CloseWindow(hwnd);
        PInvoke.DestroyWindow(hwnd);
    }
}
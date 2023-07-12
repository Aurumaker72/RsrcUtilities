using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using RsrcArchitect.ViewModels;
using RsrcArchitect.Views.WPF.Extensions;
using RsrcCore.Controls;
using RsrcCore.Geometry;
using System;

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
    private const uint DS_SETFONT = (uint)0x40L;/* User specified font for Dlg controls */
    private const uint DS_MODALFRAME = (uint)0x80L;/* Can be combined with WS_CAPTION  */
    private const uint DS_FIXEDSYS = (uint)0x0008L;
    private const uint WS_POPUP = (uint)0x80000000L;
    private const uint WS_CAPTION = (uint)0x00C00000L;/* WS_BORDER | WS_DLGFRAME  */
    private const uint WS_SYSMENU = (uint)0x00080000L;
    private const uint WM_CLOSE = 0x0010;
    private const uint WM_INITDIALOG = 0x0110;
    private const uint WM_SETFONT = 0x0030;
    // ReSharper restore InconsistentNaming
    // ReSharper restore IdentifierTypo

    private static unsafe HWND CreateControl(HWND dialogHwnd, Control control, Rectangle rectangle)
    {
        WINDOW_STYLE style = WINDOW_STYLE.WS_VISIBLE | WINDOW_STYLE.WS_CHILD;
        if (!control.IsEnabled)
        {
            style |= WINDOW_STYLE.WS_DISABLED;
        }

        if (control is Button button)
        {
            return PInvoke.CreateWindowEx(0, "BUTTON".ToPcwstr(), button.Caption.ToPcwstr(),
                style, rectangle.X, rectangle.Y, rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is TextBox textBox)
        {
            if (!textBox.IsWriteable) style |= (WINDOW_STYLE)ES_READONLY;
            if (textBox.AllowHorizontalScroll) style |= (WINDOW_STYLE)ES_AUTOHSCROLL;

            return PInvoke.CreateWindowEx(WINDOW_EX_STYLE.WS_EX_CLIENTEDGE, "EDIT".ToPcwstr(), "".ToPcwstr(),
                style, rectangle.X, rectangle.Y, rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is CheckBox checkBox)
        {
            return PInvoke.CreateWindowEx(0, "BUTTON".ToPcwstr(), checkBox.Caption.ToPcwstr(),
                style | (WINDOW_STYLE)BS_AUTOCHECKBOX, rectangle.X,
                rectangle.Y, rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is GroupBox groupBox)
        {
            return PInvoke.CreateWindowEx(0, "BUTTON".ToPcwstr(), groupBox.Caption.ToPcwstr(),
                style | (WINDOW_STYLE)BS_GROUPBOX, rectangle.X, rectangle.Y,
                rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is ComboBox comboBox)
        {
            return PInvoke.CreateWindowEx(0, "ComboBox".ToPcwstr(), "?".ToPcwstr(),
                style | (WINDOW_STYLE)CBS_DROPDOWN | (WINDOW_STYLE)CBS_DROPDOWNLIST, rectangle.X, rectangle.Y,
                rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        else if (control is Label label)
        {
            return PInvoke.CreateWindowEx(0, "STATIC".ToPcwstr(), label.Caption.ToPcwstr(),
                style, rectangle.X, rectangle.Y, rectangle.Width,
                rectangle.Height, dialogHwnd, HMENU.Null, _instance);
        }
        return HWND.Null;
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
       

        DLGTEMPLATE template = new()
        {
            x = 0,
            y = 0,
            cx = (short)dialogViewModel.Width,
            cy = (short)dialogViewModel.Height,
            style = DS_SETFONT | DS_MODALFRAME | DS_FIXEDSYS | WS_POPUP | WS_CAPTION | WS_SYSMENU,
            cdit = 0,
        };

        var defaultFont = PInvoke.GetStockObject(Windows.Win32.Graphics.Gdi.GET_STOCK_OBJECT_FLAGS.DEFAULT_GUI_FONT);

        var hwnd = PInvoke.DialogBoxIndirectParam(_instance, &template, HWND.Null, (hwnd, msg, _, _) =>
        {
            if (msg == WM_INITDIALOG)
            {
                foreach (var (key, value) in dialogViewModel.DoLayout())
                {
                    var controlHwnd = CreateControl(hwnd, key, value);

                    PInvoke.SendMessage(controlHwnd, WM_SETFONT, new WPARAM((nuint)defaultFont.Value), new LPARAM(1));
                }
                PInvoke.SetWindowText(hwnd, dialogViewModel.Caption.ToPcwstr());
            }

            if (msg == WM_CLOSE)
            {
                PInvoke.EndDialog(hwnd, 0);
            }

            return 0;
        }, new LPARAM(0));
    }
}
using System.Diagnostics.Contracts;
using System.Text;
using RsrcUtilities.Controls;
using RsrcUtilities.Extensions;
using RsrcUtilities.Interfaces;

namespace RsrcUtilities.Implementations;

/// <summary>
///     The default dialog serializer
/// </summary>
public class DefaultDialogSerializer : IDialogSerializer
{
    /// <inheritdoc />
    [Pure]
    public string Serialize(Dialog dialog)
    {
        // deep-copy the dialog and all of its contents, because we overwrite it with nonsense in the layout pass
        return DoSerialize(dialog.Copy());
    }

    private static string DoSerialize(Dialog dialog)
    {
        StringBuilder stringBuilder = new();

        // IDD_ABOUTBOX DIALOGEX 0, 0, 300,  200
        // Identifier   Type     X  Y  Width Height
        stringBuilder.AppendLine($"{dialog.Identifier} DIALOGEX 0, 0, {dialog.Width}, {dialog.Height}");

        List<string> dialogStyles = new();

        if (dialog.Chrome == Dialog.Chromes.Default)
        {
            // default "thick", no-maximize, no-minimize
            // DS_SETFONT | DS_MODALFRAME | DS_FIXEDSYS | WS_POPUP | WS_CAPTION | WS_SYSMENU
            dialogStyles.Add("DS_SETFONT");
            dialogStyles.Add("DS_MODALFRAME");
            dialogStyles.Add("DS_FIXEDSYS");
            dialogStyles.Add("WS_POPUP");
            dialogStyles.Add("WS_CAPTION");
            dialogStyles.Add("WS_SYSMENU");
        }

        stringBuilder.AppendLine($"STYLE {string.Join(" | ", dialogStyles)}");

        // syntax error avoidance
        if (dialog.Caption.Contains('"')) throw new ArgumentException("Default caption contains illegal characters");

        stringBuilder.AppendLine($"CAPTION \"{dialog.Caption}\"");

        // FONT      8,   "MS Shell Dlg", 0, 0, 0x1
        // Specifier Size Font family     Unknown?
        stringBuilder.AppendLine($"FONT {dialog.FontSize}, \"{dialog.FontFamily}\", 0, 0, 0x1");

        stringBuilder.AppendLine("BEGIN");

        // do layout pass on controls first
        // [1] -> flatten margins
        // TODO: [2] -> process horizontal and vertical alignment modes
        foreach (var node in dialog.Root)
        {
            var control = node.Data;

            control.MarginLeft = node.GetParents().Sum(x => x.Data.MarginLeft) + control.MarginLeft;
            control.MarginTop = node.GetParents().Sum(x => x.Data.MarginTop) + control.MarginTop;
        }
        
        // layout done, generate rc now
        foreach (var node in dialog.Root)
        {
            var control = node.Data;

            switch (control)
            {
                case Button button:
                    stringBuilder.AppendLine(
                        $"CONTROL \"{button.Caption}\", {button.Identifier}, \"Button\", WS_TABSTOP, {control.MarginLeft}, {control.MarginTop}, {button.Width}, {button.Height}");
                    break;
                case TextBox textBox:
                {
                    List<string> textBoxStyles = new();
                    if (textBox.InputFilter.HasFlag(TextBox.InputFilters.Numbers)) textBoxStyles.Add("ES_NUMBER");
                    if (!textBox.IsWriteable) textBoxStyles.Add("ES_READONLY");
                    if (textBox.AllowHorizontalScroll) textBoxStyles.Add("ES_AUTOHSCROLL");

                    var textBoxLine =
                        $"EDITTEXT {textBox.Identifier}, {control.MarginLeft}, {control.MarginTop}, {textBox.Width}, {textBox.Height}";
                    if (textBoxStyles.Count > 0) textBoxLine += $", {string.Join(" | ", textBoxStyles)}";
                    stringBuilder.AppendLine(textBoxLine);

                    break;
                }
                case Panel panel:
                {
                    // no-op, for now
                    break;
                }
                default:
                    throw new Exception($"Failed to serialize {control.GetType()}");
            }
        }

        stringBuilder.AppendLine("END");
        return stringBuilder.ToString();
    }
}
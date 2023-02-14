using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
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

    /// <inheritdoc />
    [Pure]
    public Dialog Deserialize(string serialized)
    {
        var dialog = new Dialog();

        var lines = serialized.Split(Environment.NewLine);

        // resolve the simple dialog info first
        
        // IDD_ABOUTBOX DIALOGEX 0, 0, 300, 200
        // [0]          [1]     [2][3][4]  [5]
        var bareDialogDefinition = Regex.Replace(lines[0].Replace(',', ' '), @"\s+", " ").Split(' ');
        
        dialog.Identifier = bareDialogDefinition[0];
        dialog.Width = int.Parse(bareDialogDefinition[4]);
        dialog.Height = int.Parse(bareDialogDefinition[5]);

        var styleDefinition = lines[1];
        var styles = styleDefinition.Replace("STYLE", "").Split('|');
        for (int i = 0; i < styles.LongLength; i++)
        {
            styles[i] = styles[i].Replace(" ", "");
        }

        if (styles.OrderBy(s => s).SequenceEqual(new []
            {
                "DS_SETFONT",
                "DS_MODALFRAME",
                "DS_FIXEDSYS",
                "WS_POPUP",
                "WS_CAPTION",
                "WS_SYSMENU",
            }.OrderBy(t => t)))
        {
            dialog.Chrome = Dialog.Chromes.Default;
        }
        else
        {
            throw new Exception($"Style sequence couldn't be reversed into known type");
        }

        var caption = lines[2].Replace("CAPTION", "");
        var quoteStartIndex = caption.IndexOf('"') + 1;
        var quoteEndIndex = caption.LastIndexOf('"');
        caption = caption.Substring(quoteStartIndex, quoteEndIndex - quoteStartIndex);

        var fontSize = int.Parse(lines[3].Split(' ')[1].Replace(",", ""));
        var fontFamily = lines[3];
        quoteStartIndex = fontFamily.IndexOf('"') + 1;
        quoteEndIndex = fontFamily.LastIndexOf('"');
        fontFamily = fontFamily.Substring(quoteStartIndex, quoteEndIndex - quoteStartIndex);

        var controlDefinitionLines = new ArraySegment<string>(lines, 5, lines.Length - (5 + 2));
        
        
        
        throw new NotImplementedException();
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
            dialogStyles.AddRange(new []
            {
                "DS_SETFONT",
                "DS_MODALFRAME",
                "DS_FIXEDSYS",
                "WS_POPUP",
                "WS_CAPTION",
                "WS_SYSMENU",
            });
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

            control.MarginLeft = node.GetParents().Sum(x => x.Data.MarginLeft + x.Data.RequiredPaddingLeft) + control.MarginLeft;
            control.MarginTop = node.GetParents().Sum(x => x.Data.MarginTop + x.Data.RequiredPaddingTop) + control.MarginTop;
        }
        
        // layout done, generate rc now
        foreach (var node in dialog.Root)
        {
            var control = node.Data;

            switch (control)
            {
                case Button button:
                    stringBuilder.AppendLine(
                        $"CONTROL \"{button.Caption}\", {control.Identifier}, \"Button\", WS_TABSTOP, {control.MarginLeft}, {control.MarginTop}, {control.Width}, {control.Height}");
                    break;
                case TextBox textBox:
                {
                    List<string> textBoxStyles = new();
                    if (textBox.InputFilter.HasFlag(TextBox.InputFilters.Numbers)) textBoxStyles.Add("ES_NUMBER");
                    if (!textBox.IsWriteable) textBoxStyles.Add("ES_READONLY");
                    if (textBox.AllowHorizontalScroll) textBoxStyles.Add("ES_AUTOHSCROLL");

                    var textBoxLine =
                        $"EDITTEXT {control.Identifier}, {control.MarginLeft}, {control.MarginTop}, {textBox.Width}, {textBox.Height}";
                    if (textBoxStyles.Count > 0) textBoxLine += $", {string.Join(" | ", textBoxStyles)}";
                    stringBuilder.AppendLine(textBoxLine);

                    break;
                }
                case Panel panel:
                {
                    // no-op, for now
                    break;
                }
                case GroupBox groupBox:
                {
                    // GROUPBOX        "Static",IDC_STATIC,179,42,75,103
                    stringBuilder.AppendLine(
                        $"GROUPBOX \"{groupBox.Caption}\", {control.Identifier}, {control.MarginLeft}, {control.MarginTop}, {control.Width}, {control.Height}");

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
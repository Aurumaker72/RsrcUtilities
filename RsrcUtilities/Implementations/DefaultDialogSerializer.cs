using System.Text;
using RsrcUtilities.Controls;
using RsrcUtilities.Interfaces;

namespace RsrcUtilities.Implementations;

/// <summary>
///     The default dialog serializer
/// </summary>
public class DefaultDialogSerializer : IDialogSerializer
{
    /// <inheritdoc />
    public string Serialize(Dialog dialog)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"{dialog.Identifier} DIALOGEX 0, 0, {dialog.Width}, {dialog.Height}");

        List<string> dialogStyles = new();

        if (dialog.Chrome == Dialog.Chromes.Default)
        {
            dialogStyles.Add("DS_SETFONT");
            dialogStyles.Add("DS_MODALFRAME");
            dialogStyles.Add("DS_FIXEDSYS");
            dialogStyles.Add("WS_POPUP");
            dialogStyles.Add("WS_CAPTION");
            dialogStyles.Add("WS_SYSMENU");
        }

        stringBuilder.AppendLine($"STYLE {string.Join(" | ", dialogStyles)}");

        if (dialog.Caption.Contains('"')) throw new ArgumentException("Default caption contains illegal characters");

        stringBuilder.AppendLine($"CAPTION \"{dialog.Caption}\"");
        stringBuilder.AppendLine($"FONT {dialog.FontSize}, \"{dialog.FontFamily}\", 0, 0, 0x1");
        stringBuilder.AppendLine("BEGIN");

        foreach (var node in dialog.Root)
        {
            var control = node.Data;

            if (control.Identifier.Any(char.IsWhiteSpace))
                throw new Exception("Whitespace is not allowed in identifier strings");

            int absoluteControlLeft = node.GetParents().Sum(x => x.Data.MarginLeft) + control.MarginLeft;
            int absoluteControlTop = node.GetParents().Sum(x => x.Data.MarginTop) + control.MarginTop;
            
            switch (control)
            {
                case Button button:
                    stringBuilder.AppendLine(
                        $"CONTROL \"{button.Caption}\", {button.Identifier}, \"Button\", WS_TABSTOP, {absoluteControlLeft}, {absoluteControlTop}, {button.Width}, {button.Height}");
                    break;
                case TextBox textBox:
                {
                    List<string> textBoxStyles = new();
                    if (textBox.InputFilter.HasFlag(TextBox.InputFilters.Numbers)) textBoxStyles.Add("ES_NUMBER");
                    if (!textBox.IsWriteable) textBoxStyles.Add("ES_READONLY");
                    if (textBox.AllowHorizontalScroll) textBoxStyles.Add("ES_AUTOHSCROLL");

                    var textBoxLine =
                        $"EDITTEXT {textBox.Identifier}, {absoluteControlLeft}, {absoluteControlTop}, {textBox.Width}, {textBox.Height}";
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
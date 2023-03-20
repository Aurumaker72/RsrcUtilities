using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using RsrcCore.Controls;
using RsrcCore.Extensions;
using RsrcCore.Geometry.Structs;
using RsrcCore.Serializers.Interfaces;

namespace RsrcCore.Serializers.Implementations;

/// <summary>
///     A dialog serializer specialized in the .rc format
/// </summary>
public class RcDialogSerializer : IDialogSerializer
{
    /// <inheritdoc />
    [Pure]
    public string Serialize(Dictionary<Control, Rectangle> flattenedControls, Dialog dialog)
    {
        // deep-copy the dialog and all of its contents, because we overwrite it with nonsense in the layout pass
        return DoSerialize(flattenedControls, dialog.Copy());
    }

    /// <inheritdoc />
    [Pure]
    public Dialog Deserialize(string serialized)
    {
        var dialog = new Dialog();

        var lines = serialized.Split(Environment.NewLine);
        var tokens = Tokenize(lines);

        throw new NotImplementedException();
    }

    private IEnumerable<Token> Tokenize(IEnumerable<string> lines)
    {
        List<Token> tokens = new();

        var enumerable = lines as string[] ?? lines.ToArray();

        var preludes = Regex.Replace(enumerable.ElementAt(0).Replace(',', ' '), @"\s+", " ").Split(' ');

        tokens.Add(new Token(Token.Types.Identifier, preludes[0]));
        tokens.Add(new Token(Token.Types.Keyword, preludes[1]));
        tokens.Add(new Token(Token.Types.LiteralNumber, preludes[2]));
        tokens.Add(new Token(Token.Types.LiteralNumber, preludes[3]));
        tokens.Add(new Token(Token.Types.LiteralNumber, preludes[4]));
        tokens.Add(new Token(Token.Types.LiteralNumber, preludes[5]));

        var styles = enumerable.ElementAt(1).Replace("| ", "").Split(' ');

        tokens.Add(new Token(Token.Types.Keyword, styles[0]));
        tokens.AddRange(styles[1..].Select(style => new Token(Token.Types.BuiltinType, style)));

        var captions = enumerable.ElementAt(2).Replace("\"", "").Split(' ');
        tokens.Add(new Token(Token.Types.Keyword, captions[0]));
        tokens.Add(new Token(Token.Types.LiteralString, captions[1]));

        var fonts = enumerable.ElementAt(3).Replace("\"", "").Split(' ');
        tokens.Add(new Token(Token.Types.Keyword, captions[0]));
        tokens.Add(new Token(Token.Types.LiteralNumber, captions[1]));
        tokens.Add(new Token(Token.Types.LiteralString, captions[2]));
        tokens.Add(new Token(Token.Types.LiteralNumber, captions[3]));
        tokens.Add(new Token(Token.Types.LiteralNumber, captions[4]));
        tokens.Add(new Token(Token.Types.LiteralNumber, captions[5]));

        var begin = enumerable.ElementAt(4);
        tokens.Add(new Token(Token.Types.Keyword, begin));

        var controls = enumerable[new Range(4, enumerable.Length - 1)];
        foreach (var control in controls)
        {
            var parameters = enumerable.ElementAt(3).Replace("\"", "").Split(' ');
            tokens.Add(new Token(Token.Types.Keyword, captions[0]));
        }


        var end = enumerable.ElementAt(enumerable.Length - 1);
        tokens.Add(new Token(Token.Types.Keyword, end));

        return tokens;
    }

    private static string DoSerialize(Dictionary<Control, Rectangle> flattenedControls, Dialog dialog)
    {
        StringBuilder stringBuilder = new();

        // IDD_ABOUTBOX DIALOGEX 0, 0, 300,  200
        // Identifier   Type     X  Y  X Y
        stringBuilder.AppendLine($"{dialog.Identifier} DIALOGEX 0, 0, {dialog.Width}, {dialog.Height}");

        List<string> dialogStyles = new();

        if (dialog.Chrome == Dialog.Chromes.Default)
            // default "thick", no-maximize, no-minimize
            // DS_SETFONT | DS_MODALFRAME | DS_FIXEDSYS | WS_POPUP | WS_CAPTION | WS_SYSMENU
            dialogStyles.AddRange(new[]
            {
                "DS_SETFONT",
                "DS_MODALFRAME",
                "DS_FIXEDSYS",
                "WS_POPUP",
                "WS_CAPTION",
                "WS_SYSMENU"
            });

        stringBuilder.AppendLine($"STYLE {string.Join(" | ", dialogStyles)}");

        // syntax error avoidance
        if (dialog.Caption.Contains('"')) throw new Exception("Dialog caption contained quotation marks");

        stringBuilder.AppendLine($"CAPTION \"{dialog.Caption}\"");

        // FONT      8,   "MS Shell Dlg", 0, 0, 0x1
        // Specifier Size Font family     Unknown?
        stringBuilder.AppendLine($"FONT {dialog.FontSize}, \"{dialog.FontFamily}\", 0, 0, 0x1");

        stringBuilder.AppendLine("BEGIN");

        // layout done, generate rc now
        foreach (var pair in flattenedControls)
        {
            var control = pair.Key;
            var rectangle = pair.Value;

            switch (control)
            {
                case Button button:
                    stringBuilder.AppendLine(
                        $"CONTROL \"{button.Caption}\", {control.Identifier}, \"Button\", WS_TABSTOP, {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}");
                    break;
                case CheckBox checkBox:
                    stringBuilder.AppendLine(
                        $"CONTROL \"{checkBox.Caption}\", {control.Identifier}, \"Button\", BS_AUTOCHECKBOX | WS_TABSTOP, {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}");
                    break;
                case TextBox textBox:
                {
                    List<string> textBoxStyles = new();
                    if (textBox.InputFilter.HasFlag(TextBox.InputFilters.Numbers)) textBoxStyles.Add("ES_NUMBER");
                    if (!textBox.IsWriteable) textBoxStyles.Add("ES_READONLY");
                    if (textBox.AllowHorizontalScroll) textBoxStyles.Add("ES_AUTOHSCROLL");

                    var textBoxLine =
                        $"EDITTEXT {control.Identifier}, {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}";
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
                        $"GROUPBOX \"{groupBox.Caption}\", {control.Identifier}, {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}");

                    break;
                }
                default:
                    throw new Exception($"Failed to serialize {control.GetType()}");
            }
        }

        stringBuilder.AppendLine("END");
        return stringBuilder.ToString();
    }

    private readonly struct Token
    {
        public enum Types
        {
            Keyword,
            BuiltinType,
            Identifier,
            LiteralString,
            LiteralNumber
        }

        public readonly Types Type;
        public readonly string Text;

        public Token(Types type, string text)
        {
            Type = type;
            Text = text;
        }
    }
}
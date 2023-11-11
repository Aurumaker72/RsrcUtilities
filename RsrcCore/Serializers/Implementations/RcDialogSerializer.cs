using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using RsrcCore.Controls;
using RsrcCore.Extensions;
using RsrcCore.Geometry;
using RsrcCore.Serializers.Interfaces;

namespace RsrcCore.Serializers.Implementations;

/// <summary>
///     A dialog serializer specialized in the .rc format
/// </summary>
public class RcDialogSerializer : IDialogSerializer
{
    /// <summary>
    /// Whether a compilable rc file should be generated
    /// </summary>
    public bool GenerateCompilable { get; set; }

    /// <inheritdoc />
    [Pure]
    public string Serialize(Dictionary<Control, Rectangle> flattenedControls, Dialog dialog)
    {
        // deep-copy the dialog and all of its contents, because we overwrite it with nonsense in the layout pass
        var result = DoSerialize(flattenedControls, dialog.Copy());

        if (!GenerateCompilable)
        {
            return result;
        }

        // we also include metadata and windows.h includes

        return $"""
#include "resource.h"
#include <windows.h>
{result}
""";
    }

    /// <inheritdoc />
    [Pure]
    public Dialog Deserialize(string serialized)
    {
        var dialog = new Dialog()
        {
            Root = new TreeNode<Control>(new Panel())
        };

        var lines = serialized.Split(Environment.NewLine);

        // IDD_DIALOG DIALOGEX 0, 0, 342, 342
        var prelude = lines[0].Replace(",", "").Split(" ");
        dialog.Identifier = prelude[0];
        dialog.Width = int.Parse(prelude[4]);
        dialog.Height = int.Parse(prelude[5]);

        // CAPTION "Play Movie"
        const string captionHint = "CAPTION ";
        // we chop off one additional character on each side to remove the quotes
        dialog.Caption = lines[2].Substring(captionHint.Length + 1, lines[2].Length - captionHint.Length - 2);


        var controlLines = lines[new Range(new Index(5), new Index(1, true))].Select(x => x.Trim());

        foreach (var controlLine in controlLines)
        {
            // different control classes have different order of properties, so we need to work conditionally
            var controlClass = controlLine.Split(" ")[0];
            
            if (controlClass == "PUSHBUTTON")
            {
                var propertiesStartIndex = controlLine.IndexOf("\"", StringComparison.Ordinal);
                var controlProperties = controlLine[propertiesStartIndex..];
                var captionEndIndex = controlProperties.IndexOf('"', 1);
                var caption = controlProperties[1..captionEndIndex];

                var rest = controlProperties[captionEndIndex..];
                var restProperties = rest.Split(",");

                var identifier = restProperties[1];
                var x = int.Parse(restProperties[2]);
                var y = int.Parse(restProperties[3]);
                var width = int.Parse(restProperties[4]);
                var height = int.Parse(restProperties[5]);

                dialog.Root.AddChild(new Button()
                {
                    Identifier = identifier,
                    Rectangle = new Rectangle(x, y, width, height),
                    Caption = caption
                });
            }
        }
        
        return dialog;
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
                        $"CONTROL \"{button.Caption}\", {control.Identifier}, \"Button\", WS_TABSTOP {(control.IsEnabled ? "" : "| WS_DISABLED")}, {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}");
                    break;
                case CheckBox checkBox:
                    stringBuilder.AppendLine(
                        $"CONTROL \"{checkBox.Caption}\", {control.Identifier}, \"Button\", BS_AUTOCHECKBOX | WS_TABSTOP {(control.IsEnabled ? "" : "| WS_DISABLED")}, {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}");
                    break;
                case TextBox textBox:
                {
                    List<string> textBoxStyles = new();
                    if (textBox.InputFilter.HasFlag(TextBox.InputFilters.Numbers)) textBoxStyles.Add("ES_NUMBER");
                    if (!textBox.IsWriteable) textBoxStyles.Add("ES_READONLY");
                    if (textBox.AllowHorizontalScroll) textBoxStyles.Add("ES_AUTOHSCROLL");
                    if (!control.IsEnabled) textBoxStyles.Add("WS_DISABLED");

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
                    List<string> comboBoxStyles = new();
                    if (!control.IsEnabled) comboBoxStyles.Add("WS_DISABLED");

                    var comboBoxLine =
                        $"GROUPBOX \"{control.Identifier}\", {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}";
                    if (comboBoxStyles.Count > 0) comboBoxLine += $", {string.Join(" | ", comboBoxStyles)}";
                    stringBuilder.AppendLine(comboBoxLine);
                    
                    break;
                }
                case Label label:
                {
                    // LTEXT           "Static",IDC_STATIC,207,46,50,8
                    stringBuilder.AppendLine(
                        $"LTEXT \"{label.Caption}\", {control.Identifier}, {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}");
                    break;
                }
                case ComboBox comboBox:
                {
                    // COMBOBOX        IDC_COMBO1,371,129,151,21,CBS_DROPDOWN | CBS_SORT | WS_VSCROLL | WS_TABSTOP
                    List<string> comboBoxStyles = new();
                    comboBoxStyles.AddRange(new[]
                    {
                        "WS_TABSTOP",
                        "CBS_DROPDOWN",
                        "WS_VSCROLL",
                    });
                    if (comboBox.IsSorted)
                    {
                        comboBoxStyles.Add("CBS_SORT");
                    }
                    if (!control.IsEnabled) comboBoxStyles.Add("WS_DISABLED");

                    var line = $"COMBOBOX {control.Identifier}, {rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}";
                    if (comboBoxStyles.Count > 0) line += $", {string.Join(" | ", comboBoxStyles)}";
                    stringBuilder.AppendLine(line);

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

        public override string ToString()
        {
            return $"{Type}: {Text}";
        }
    }
}
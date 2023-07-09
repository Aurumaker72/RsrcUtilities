using System.Globalization;
using System.Text;
using RsrcCore.Controls;
using RsrcCore.Geometry;
using RsrcCore.Serializers.Interfaces;

namespace RsrcCore.Serializers.Implementations;

/// <summary>
///     A dialog serializer specialized in https://github.com/Aurumaker72/mupen-lua-ugui control definitions
/// </summary>
public class UguiDialogSerializer : IDialogSerializer
{

    
    
    public string Serialize(Dictionary<Control, Rectangle> flattenedControls, Dialog dialog)
    {
        StringBuilder stringBuilder = new();

        int i = 0;
        foreach (var (key, value) in flattenedControls)
        {
            if (key is Button button)
            {
                stringBuilder.AppendLine($@"
                    Mupen_lua_ugui.button({{
                          uid = {i},
                          is_enabled = true,
                          rectangle = {{ x = {value.X}, y = {value.Y}, width = {value.Width}, height = {value.Height} }},
                          text = ""{button.Caption}"",
                    }})
                ");
            } else if (key is TextBox textBox)
            {
                stringBuilder.AppendLine($@"
                    Mupen_lua_ugui.textbox({{
                          uid = {i},
                          is_enabled = {textBox.IsEnabled.ToString(CultureInfo.InvariantCulture).ToLower()},
                          rectangle = {{ x = {value.X}, y = {value.Y}, width = {value.Width}, height = {value.Height} }},
                          text = """",
                    }})
                ");
            }
            else if (key is CheckBox checkBox)
            {
                stringBuilder.AppendLine($@"
                    Mupen_lua_ugui.toggle_button({{
                          uid = {i},
                          is_enabled = true,
                          rectangle = {{ x = {value.X}, y = {value.Y}, width = {value.Width}, height = {value.Height} }},
                          text = ""{checkBox.Caption}"",
                          is_checked = false,
                    }})
                ");
            }
            else if (key is ComboBox comboBox)
            {
                stringBuilder.AppendLine($@"
                    Mupen_lua_ugui.combobox({{
                          uid = {i},
                          is_enabled = true,
                          rectangle = {{ x = {value.X}, y = {value.Y}, width = {value.Width}, height = {value.Height} }},
                          items = {{}},
                          selected_index = 0,
                    }})
                ");
            }
            else if (key is GroupBox groupBox)
            {
                stringBuilder.AppendLine($@"

BreitbandGraphics.renderers.d2d.draw_rectangle({{ x = {value.X}, y = {value.Y}, width = {value.Width}, height = {value.Height} }}, 
BreitbandGraphics.hex_to_color(""#DCDCDC""), 1)

BreitbandGraphics.renderers.d2d.draw_text({{ x = {value.X}, y = {value.Y}, width = {value.Width}, height = {value.Height} }}, 
""start"",
""start"", 
{{}}, 
BreitbandGraphics.colors.white, 
Mupen_lua_ugui.stylers.windows_10.font_size, 
Mupen_lua_ugui.stylers.windows_10.font_name, 
""{groupBox.Caption}"")
                ");
            }
            i++;
        }

        return stringBuilder.ToString();

    }

    public Dialog Deserialize(string serialized)
    {
        throw new NotImplementedException();
    }
}
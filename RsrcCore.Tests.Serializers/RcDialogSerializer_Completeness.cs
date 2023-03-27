using System.Text.RegularExpressions;
using RsrcCore.Controls;
using RsrcCore.Geometry.Enums;
using RsrcCore.Geometry.Structs;
using RsrcCore.Layout.Implementations;
using RsrcCore.Serializers.Implementations;

namespace RsrcCore.UnitTests.Serializers;

public class RcDialogSerializer_Completeness
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test_Serialization()
    {
        var dialog = new Dialog
        {
            Identifier = "IDD_TEST",
            Width = 800,
            Height = 600,
            Root = new TreeNode<Control>(new Panel
            {
                Rectangle = new Rectangle(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignments.Stretch,
                VerticalAlignment = VerticalAlignments.Stretch
            })
        };

        
        dialog.Root.AddChild(new Button { Identifier = "IDC_BUTTON" });
        dialog.Root.AddChild(new CheckBox { Identifier = "IDC_CHECKBOX" });
        dialog.Root.AddChild(new ComboBox { Identifier = "IDC_COMBOBOX" });
        dialog.Root.AddChild(new GroupBox { Identifier = "IDC_GROUPBOX" });
        dialog.Root.AddChild(new TextBox { Identifier = "IDC_TEXTBOX" });
        dialog.Root.AddChild(new Label { Identifier = "IDC_LABEL" });

        _ = new RcDialogSerializer().Serialize(new DefaultLayoutEngine().DoLayout(dialog), dialog);
    }
}
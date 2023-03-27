using RsrcCore;
using RsrcCore.Controls;
using RsrcCore.Generators.Implementations;
using RsrcCore.Geometry.Enums;
using RsrcCore.Geometry.Structs;
using RsrcCore.Layout.Implementations;
using RsrcCore.Serializers.Implementations;

var dialog = new Dialog
{
    Identifier = "IDD_ABOUTBOX",
    Width = 100,
    Height = 100
};

var root = new TreeNode<Control>(new GroupBox
{
    Identifier = "IDC_GROUPBOX",
    Caption = "Look at me!",
    Rectangle = new Rectangle(0, 0, 0, 0),
    HorizontalAlignment = HorizontalAlignments.Stretch,
    VerticalAlignment = VerticalAlignments.Stretch
});

root.AddChild(new CheckBox
{
    Identifier = "IDC_CENTER_CENTER_CHECKBOX",
    Caption = "Center Center",
    Rectangle = new Rectangle(0, 0, 40, 20),
    HorizontalAlignment = HorizontalAlignments.Center,
    VerticalAlignment = VerticalAlignments.Center
});
root.AddChild(new Button
{
    Identifier = "IDC_LEFT_TOP_BUTTON",
    Caption = "Left Top",
    Rectangle = new Rectangle(0, 0, 40, 20),
    HorizontalAlignment = HorizontalAlignments.Left,
    VerticalAlignment = VerticalAlignments.Top
});
root.AddChild(new Button
{
    Identifier = "IDC_RIGHT_BOTTOM_BUTTON",
    Caption = "Right Bottom",
    Rectangle = new Rectangle(0, 0, 40, 20),
    HorizontalAlignment = HorizontalAlignments.Right,
    VerticalAlignment = VerticalAlignments.Bottom
});

dialog.Root = root;

var serializedDialog = new RcDialogSerializer().Serialize(new DefaultLayoutEngine().DoLayout(dialog), dialog);

var deserializedDialog = new RcDialogSerializer().Deserialize(serializedDialog);

var generatedResource = new CxxHeaderInformationGenerator().Generate(dialog.Root.Flatten());

// File.WriteAllText("Resource.h", generatedResource);
// File.WriteAllText("rsrc.rc", serializedDialog);
//
// Console.WriteLine(generatedResource);
// Console.WriteLine(serializedDialog);
Console.ReadLine();
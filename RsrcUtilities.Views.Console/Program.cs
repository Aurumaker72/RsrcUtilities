using RsrcUtilities;
using RsrcUtilities.Controls;
using RsrcUtilities.Controls.Enums;
using RsrcUtilities.Implementations;

var dialog = new Dialog
{
    Identifier = "IDD_ABOUTBOX"
};

var root = new TreeNode<Control>(new GroupBox()
{
    Identifier = "IDC_GROUPBOX",
    Caption = "Stuff",
    MarginLeft = 5,
    MarginTop = 5,
    Width = 200,
    Height = 200,
});

root.AddChild(new Button
{
    Identifier = "IDC_BUTTON",
    MarginLeft = 0,
    MarginTop = 0,
    Width = 80,
    Height = 20,
    Caption = "Hello World!",
    HorizontalAlignment = HorizontalAlignments.Center,
    VerticalAlignment = VerticalAlignments.Center
});
dialog.Root = root;

var serializedDialog = new DefaultDialogSerializer().Serialize(dialog);
var generatedResource = new DefaultResourceGenerator().Generate(dialog.Root);

File.WriteAllText("Resource.h", generatedResource);
File.WriteAllText("rsrc.rc", serializedDialog);

new DefaultDialogSerializer().Deserialize(serializedDialog);

Console.WriteLine(generatedResource);
Console.WriteLine(serializedDialog);
Console.ReadLine();
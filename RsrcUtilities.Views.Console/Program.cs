using RsrcUtilities;
using RsrcUtilities.Controls;
using RsrcUtilities.Controls.Enums;
using RsrcUtilities.Controls.Layout;
using RsrcUtilities.Implementations;

var dialog = new Dialog
{
    Identifier = "IDD_ABOUTBOX",
    Width = 100,
    Height = 100,
};
var root = new TreeNode<Control>(new Button
{
    Identifier = "IDC_BUTTON",
    Margin = new Thickness(20, 20, 20, 20),
    Caption = "Hello World!",
    HorizontalAlignment = HorizontalAlignments.Stretch,
    VerticalAlignment = VerticalAlignments.Stretch
});

root.AddChild(new Button
{
    Identifier = "IDC_BUTTON2",
    Margin = new Thickness(20, 20, 20, 20),
    Caption = "Hi!",
    HorizontalAlignment = HorizontalAlignments.Stretch,
    VerticalAlignment = VerticalAlignments.Stretch
});

dialog.Root = root;

var serializedDialog = new DefaultDialogSerializer().Serialize(dialog);
var generatedResource = new DefaultResourceGenerator().Generate(dialog.Root);

File.WriteAllText("Resource.h", generatedResource);
File.WriteAllText("rsrc.rc", serializedDialog);

// new DefaultDialogSerializer().Deserialize(serializedDialog);

Console.WriteLine(generatedResource);
Console.WriteLine(serializedDialog);
Console.ReadLine();
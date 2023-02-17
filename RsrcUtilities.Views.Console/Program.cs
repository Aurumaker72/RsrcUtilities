
using RsrcUtilities;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Enums;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.Layout.Implementations;
using RsrcUtilities.Serializers.Implementations;

var dialog = new Dialog
{
    Identifier = "IDD_ABOUTBOX",
    Width = 200,
    Height = 100
};
var root = new TreeNode<Control>(new Button
{
    Identifier = "IDC_BUTTON",
    Margin = new Thickness(20, 20, 160, 60),
    Caption = "Hello World!",
    HorizontalAlignment = HorizontalAlignments.Stretch,
    VerticalAlignment = VerticalAlignments.Stretch
});

dialog.Root = root;

var serializedDialog = new DefaultDialogSerializer().Serialize(new DefaultLayoutEngine(), dialog);
var generatedResource = new DefaultResourceGenerator().Generate(dialog.Root);

File.WriteAllText("Resource.h", generatedResource);
File.WriteAllText("rsrc.rc", serializedDialog);

Console.WriteLine(generatedResource);
Console.WriteLine(serializedDialog);
Console.ReadLine();
using RsrcUtilities;
using RsrcUtilities.Controls;
using RsrcUtilities.Implementations;

var dialog = new Dialog
{
    Identifier = "IDD_ABOUTBOX"
};

var root = new TreeNode<Control>(new Panel()
{
    MarginLeft = 80,
    MarginTop = 20,
});
root.AddChild(new Button
{
    Identifier = "IDC_BUTTON",
    MarginLeft = 0,
    MarginTop = 0,
    Width = 80,
    Height = 20,
    Caption = "Hello World!"
});
root.AddChild(new TextBox
{
    Identifier = "IDC_TEXTBOX",
    MarginLeft = 0,
    MarginTop = 40,
    Width = 80,
    Height = 20
});

dialog.Root = root;

var serializedDialog = new DefaultDialogSerializer().Serialize(dialog);
var generatedResource = new DefaultResourceGenerator().Generate(dialog.Root);

File.WriteAllText("Resource.h", generatedResource);
File.WriteAllText("rsrc.rc", serializedDialog);

Console.WriteLine(generatedResource);
Console.WriteLine("----------------");
Console.WriteLine(serializedDialog);
Console.ReadLine();
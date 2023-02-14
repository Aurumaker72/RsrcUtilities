

using RsrcUtilities;
using RsrcUtilities.Controls;
using RsrcUtilities.Implementations;

var dialog = new Dialog()
{
    Identifier = "IDD_ABOUTBOX"
};

dialog.Controls.Add(new Button()
{
    Identifier = "IDC_BUTTON",
    X = 20,
    Y = 20,
    Width = 80,
    Height = 25,
    Caption = "Hello World!"
});

dialog.Controls.Add(new TextBox()
{
    Identifier = "IDC_TEXTBOX",
    X = 20,
    Y = 50,
    Width = 80,
    Height = 25,
});


var serializedDialog = new DefaultDialogSerializer().Serialize(dialog);
var generatedResource = new DefaultResourceGenerator().Generate(dialog.Controls);

File.WriteAllText("Resource.h", generatedResource);
File.WriteAllText("rsrc.rc", serializedDialog);

Console.WriteLine(generatedResource);
Console.WriteLine("----------------");
Console.WriteLine(serializedDialog);
Console.ReadLine();
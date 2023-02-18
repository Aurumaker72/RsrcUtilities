﻿using RsrcUtilities;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Enums;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.Layout.Implementations;
using RsrcUtilities.Serializers.Implementations;

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

root.AddChild(new Button
{
    Identifier = "IDC_CENTER_CENTER_BUTTON",
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

var serializedDialog = new DefaultDialogSerializer().Serialize(new DefaultLayoutEngine(), dialog);
var generatedResource = new DefaultResourceGenerator().Generate(dialog.Root);

File.WriteAllText("Resource.h", generatedResource);
File.WriteAllText("rsrc.rc", serializedDialog);

Console.WriteLine(generatedResource);
Console.WriteLine(serializedDialog);
Console.ReadLine();
using System.Text.RegularExpressions;
using RsrcCore.Controls;
using RsrcCore.Geometry.Enums;
using RsrcCore.Geometry.Structs;
using RsrcCore.Layout.Implementations;
using RsrcCore.Serializers.Implementations;

namespace RsrcCore.UnitTests.Serializers;

public class RcDialogSerializer_Correctness
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
            Width = 123,
            Height = 321,
            Root = new TreeNode<Control>(new Panel
            {
                Rectangle = new Rectangle(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignments.Stretch,
                VerticalAlignment = VerticalAlignments.Stretch
            })
        };

        var serializedDialog = new RcDialogSerializer().Serialize(new DefaultLayoutEngine().DoLayout(dialog), dialog);

        var lines = serializedDialog.Split(Environment.NewLine);
        var bareDialogDefinition = Regex.Replace(lines[0].Replace(',', ' '), @"\s+", " ").Split(' ');
        var serializedWidth = int.Parse(bareDialogDefinition[4]);
        var serializedHeight = int.Parse(bareDialogDefinition[5]);

        Assert.That(serializedWidth, Is.EqualTo(dialog.Width), "Width was incorrectly serialized");
        Assert.That(serializedHeight, Is.EqualTo(dialog.Height), "Height was incorrectly serialized");
    }
}
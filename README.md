<p align="center">
  <img width="128" align="center" src="https://user-images.githubusercontent.com/48759429/219858139-55582cd0-d03b-485b-959a-ba49db4a6498.png">
</p>

<h1 align="center">
  RsrcUtilities
</h1>
<p align="center">
  .NET class library for interacting with rc files
</p>

<img align="center" src="https://user-images.githubusercontent.com/48759429/222482582-2b253888-d5dd-45b2-a9af-2103aacc3dbb.png">

# :zap: Quickstart 
 **Step 1:** Build, then reference all `RsrcUtilities` assemblies in your project<br/>
 **Step 2:** Create a `Dialog` object
```cs
var dialog = new Dialog
{
    Identifier = "IDD_ABOUTBOX",
    Width = 100,
    Height = 100
};
```
 **Step 3:** Initialize the `Dialog`'s root node to a control
 ```cs
dialog.Root = new TreeNode<Control>(new Panel
{
    Rectangle = new Rectangle(0, 0, 0, 0),
    HorizontalAlignment = HorizontalAlignments.Stretch,
    VerticalAlignment = VerticalAlignments.Stretch
});
```
**Step 4:** Add any controls you desire
```cs
dialog.Root.AddChild(new CheckBox()
{
    Identifier = "IDC_SOME_NAME",
    Caption = "Hi",
    Rectangle = new Rectangle(0, 0, 80, 40),
    HorizontalAlignment = HorizontalAlignments.Center,
    VerticalAlignment = VerticalAlignments.Center
});
```
**Step 5:** Serialize it to the format of your liking
```cs
new RcDialogSerializer().Serialize(new DefaultLayoutEngine().DoLayout(dialog), dialog);
```

**(Optional) Step 6:** Generate additional information in the format of your liking
```cs
new CxxHeaderResourceGenerator().Generate(dialog.Root);
```

# :star: Goal
The creation of infrastructure to facilitate the design of `rc` user interfaces without the hassles of legacy abandonware.

<details>
<summary><h4>RsrcArchitect Dialog Editor</h4></summary>
<p align="center">
  <img width="128" align="center" src="https://user-images.githubusercontent.com/48759429/221374035-7500c631-3984-433e-9200-145391f4cbbe.svg">
</p>
<h1 align="center">
  RsrcArchitect
</h1>
<p align="center">
  .NET dialog designer accompanying RsrcUtilities
  <br>
  Design dialogs with an interactive, simple experience 
</p>
<p align="center">
    <img src="https://img.shields.io/badge/Requires-RsrcUtilities-gray?style=for-the-badge"/>
</p>
# Key Features
- Accurate control rendering
- Smart hittesting
- Flexible Z-Ordering

</details>





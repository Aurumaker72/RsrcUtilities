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

# :question: Why?

The legacy Visual Studio dialog designer is questionable at best, and having slowly become fed up with it, I began working on a somewhat comprehensive `rc` editing suite. 

The goal is to create an intuitive dialog designer, to speed up production of `rc`-based GUIs without the hassles of legacy abandonware.

![grafik](https://user-images.githubusercontent.com/48759429/221355392-01f1b5d0-7754-44e1-b187-a919c54c5ed7.png)

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
</details>

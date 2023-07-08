<p align="center">
  <img width="128" align="center" src="https://user-images.githubusercontent.com/48759429/219858139-55582cd0-d03b-485b-959a-ba49db4a6498.png">
</p>

<h1 align="center">
  RsrcUtilities
</h1>
<p align="center">
   .NET utility suite for working with the rc format 
</p>
<p align="center">
   RsrcUtilities consists of RsrcCore and RsrcArchitect
</p>
<br>
<img align="center" src="https://github.com/Aurumaker72/RsrcUtilities/assets/48759429/1e8b18e5-433d-425a-bbe3-ef15ec164e38.png">


# :zap: Quickstart 
1. Build, then reference `RsrcCore`

2. Create a `Dialog` object
```cs
var dialog = new Dialog
{
    Identifier = "IDD_ABOUTBOX",
    Width = 100,
    Height = 100
};
```
3. Initialize the `Dialog`'s root node to a control
 ```cs
dialog.Root = new TreeNode<Control>(new Panel
{
    Rectangle = Rectangle.Empty,
    HorizontalAlignment = Alignments.Fill,
    VerticalAlignment = Alignments.Fill
});
```
4. Add any controls you desire
```cs
dialog.Root.AddChild(new CheckBox()
{
    Identifier = "IDC_EXAMPLE",
    Caption = "Hello World!",
    Rectangle = new Rectangle(0, 0, 80, 40),
    HorizontalAlignment = Alignments.Center,
    VerticalAlignment = Alignments.Center
});

```
5. Serialize it to the format of your liking
```cs
new RcDialogSerializer().Serialize(new DefaultLayoutEngine().DoLayout(dialog), dialog);
```

6. Generate additional information in the format of your liking
```cs
new CxxHeaderResourceGenerator().Generate(dialog.Root);
```

# :star: Goal
The creation of infrastructure to facilitate the design of `rc` user interfaces without the hassles of legacy abandonware.

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
    <img src="https://img.shields.io/badge/Requires-RsrcUtilities-blue?style=for-the-badge"/>
</p>

# 💡 Features
- Intuitive camera
  - Click-drag
  - Mouse-wheel zoom
- High-precision control renderer
  - Windows 10
  - Windows 11
- Smart selection system
  - Reliably select the intended element
- Z-Ordering system
  - Reorder controls in the dialog editor
- Multitasking
  - Tabbed navigation system
- Powerful and dynamic inspector
  - Provides context-aware information 
- Always reactive
  - Everything reacts to changes immediately with your input
- Smart positioning system
  - `Freeform` allows freely positioning controls
  - `Grid` snaps controls to grid points
  - `Snap` intelligently snaps controls based on other elements (useful for lists and fluid layouts)

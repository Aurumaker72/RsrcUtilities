using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.Services;
using RsrcArchitect.ViewModels;
using RsrcArchitect.ViewModels.Messages;
using RsrcArchitect.Views.WPF.Extensions;
using RsrcArchitect.Views.WPF.Rendering;
using RsrcArchitect.Views.WPF.Services;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace RsrcArchitect.Views.WPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class MainWindow : Window, IRecipient<CanvasInvalidationMessage>
{
    private const float ZoomIncrement = 0.5f;

    private SKElement? _skElement;

    public MainViewModel MainViewModel { get; }
    public DialogRenderer DialogRenderer { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        MainViewModel = new MainViewModel(new FilesService());

        DataContext = this;

        MainViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.SelectedDialogEditorViewModel))
                OnSelectedDialogEditorChanged();
        };

        MainViewModel.DialogEditorSettingsViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(MainViewModel.DialogEditorSettingsViewModel.VisualStyle)) return;

            OnVisualStyleChanged();
        };

        OnSelectedDialogEditorChanged();
        OnVisualStyleChanged();

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    private void OnVisualStyleChanged()
    {
        // TODO: implement atlas and metadata changing
    }

    private void OnSelectedDialogEditorChanged()
    {
        _skElement = TabControl.FindElementByName<SKElement>("SkElement");
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    private void SkElement_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;
        DialogRenderer.Render(MainViewModel.DialogEditorSettingsViewModel, dialogEditorViewModel, e.Surface.Canvas);
    }

    private void SkElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        ((IInputElement)sender).CaptureMouse();
        var position = e.GetPosition((IInputElement)sender);

        dialogEditorViewModel.PointerPressCommand.Execute(new Vector2(
            (float)position.X,
            (float)position.Y));
    }

    private void SkElement_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;
        ((IInputElement)sender).ReleaseMouseCapture();
        dialogEditorViewModel.PointerReleaseCommand.Execute(null);
    }

    private void SkElement_OnMouseMove(object sender, MouseEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        var position = e.GetPosition((IInputElement)sender);
        dialogEditorViewModel.PointerMoveCommand.Execute(new Vector2(
            (float)position.X,
            (float)position.Y));
    }

    private void PositioningModeButton_OnClick(object sender, RoutedEventArgs e)
    {
        MainViewModel.DialogEditorSettingsViewModel.PositioningMode =
            MainViewModel.DialogEditorSettingsViewModel.PositioningMode.Next();
    }

    private void ZoomOutButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        dialogEditorViewModel.Scale -= ZoomIncrement;
    }

    private void ZoomInButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        dialogEditorViewModel.Scale += ZoomIncrement;
    }

    private void SkElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
            ZoomInButton_OnClick(sender, null);
        else
            ZoomOutButton_OnClick(sender, null);
    }

    public void Receive(CanvasInvalidationMessage message)
    {
        _skElement?.InvalidateVisual();
    }


    private void Preview_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        Thread thread = new(() =>
        {
            DialogLoader.ShowDialogFromRcString(dialogEditorViewModel.DialogViewModel);
        });
        thread.Start();
    }
}
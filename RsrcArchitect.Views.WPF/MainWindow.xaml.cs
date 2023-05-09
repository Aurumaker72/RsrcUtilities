using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using RsrcArchitect.Services;
using RsrcArchitect.ViewModels;
using RsrcArchitect.ViewModels.Types;
using RsrcArchitect.Views.WPF.Extensions;
using RsrcArchitect.Views.WPF.Renderers;
using RsrcArchitect.Views.WPF.Renderers.ControlRenderers;
using RsrcArchitect.Views.WPF.Services;
using RsrcCore.Controls;
using RsrcCore.Geometry.Structs;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using Wpf.Ui.Controls.Window;

namespace RsrcArchitect.Views.WPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class MainWindow : FluentWindow, ICanvasInvalidationService
{
    private const float zoomIncrement = 0.5f;
    
    private SKElement? _skElement;

    public MainViewModel MainViewModel { get; }
    public DialogRenderer DialogRenderer { get; } = new DialogRenderer();
    
    public MainWindow()
    {
        InitializeComponent();

        MainViewModel = new MainViewModel(new FilesService(), this);

        DataContext = this;

        MainViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.SelectedDialogEditorViewModel)) OnSelectedDialogEditorChanged();
        };
        
        MainViewModel.DialogEditorSettingsViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(MainViewModel.DialogEditorSettingsViewModel.VisualStyle)) return;

            OnVisualStyleChanged();
        };
        
        OnSelectedDialogEditorChanged();
        OnVisualStyleChanged();
    }

    private void OnVisualStyleChanged()
    {
        var windowInteropHelper = new WindowInteropHelper(this);
        windowInteropHelper.EnsureHandle();
        
        DialogRenderer.ObjectRenderer = MainViewModel.DialogEditorSettingsViewModel.VisualStyle switch
        {
            "windows-11" => new Windows11ObjectRenderer(),
            "windows-10" => new Windows10ObjectRenderer(),
            "native" => new NativeObjectRenderer(windowInteropHelper.Handle),
            _ => throw new ArgumentException()
        };
    }

    private void OnSelectedDialogEditorChanged()
    {
        _skElement = TabControl.FindElementByName<SKElement>("SkElement");
        (this as ICanvasInvalidationService).Invalidate();
        
    }

    void ICanvasInvalidationService.Invalidate()
    {
        _skElement?.InvalidateVisual();
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

        dialogEditorViewModel.Scale -= zoomIncrement;
    }

    private void ZoomInButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogEditorViewModel = ((FrameworkElement)sender).DataContext as DialogEditorViewModel;

        dialogEditorViewModel.Scale += zoomIncrement;
    }

    private void SkElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
            ZoomInButton_OnClick(sender, null);
        else
            ZoomOutButton_OnClick(sender, null);
    }
}
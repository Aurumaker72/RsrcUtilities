using System.Diagnostics;
using System.Numerics;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Helpers;
using RsrcArchitect.ViewModels.Messages;
using RsrcArchitect.ViewModels.Types;
using RsrcCore.Generators.Implementations;
using RsrcCore.Layout.Implementations;
using RsrcCore.Serializers.Implementations;

namespace RsrcArchitect.ViewModels;

public partial class DialogEditorViewModel
{
    [RelayCommand]
    private void Close()
    {
        WeakReferenceMessenger.Default.Send(new DialogEditorViewModelClosingMessage(this));
    }

    [RelayCommand]
    private void PointerPress(Vector2 position)
    {
        var dialogPosition = RelativePositionToDialog(position);

        // do grip-test first, then store if it hits
        var isGripHit = false;
        (Transformation transformation, Sizing sizing) transformationOperation = (Transformation.None, Sizing.Empty);
        if (SelectedNodes.Count > 0)
        {
            // if any candidate hits, we grabbed a control's grip
            transformationOperation = SelectedNodes
                .Select(x =>
                    TransformationHelper.GetCandidate(x.Data, dialogPosition, _dialogEditorSettingsViewModel.GripSize))
                .FirstOrDefault(x => x.Item1 != Transformation.None, (Transformation.None, Sizing.Empty));

            isGripHit = transformationOperation.transformation != Transformation.None;
        }

        // if no grip hits, we know we aren't starting to resize or move a control,
        // thus we can select a new one
        if (!isGripHit)
        {
            Debug.Print("No grip hit");
            var newSelectedNode = GetControlNodeAtPosition(dialogPosition);
            SelectedNodes.Clear();
            if (newSelectedNode != null)
            {
                SelectedNodes.Add(newSelectedNode);
            }
        }

        if (SelectedNodes.Count > 0)
        {
            Debug.Print("Start control transformation");
            (_transformation, _sizing) = (transformationOperation.transformation, transformationOperation.sizing);
            _transformationStartRectangles = SelectedNodes.Select(x => x.Data.Rectangle).ToList();
            _transformationStartPointerPosition = dialogPosition;
        }
        else
        {
            // no node caught but stil clicked,
            // so start translating the camera
            Debug.Print("Start pan");
            _isPanning = true;
            _panStartPointerPosition = position;
            _panStartTranslation = Translation;
        }

        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand]
    private void PointerRelease()
    {
        _isPanning = false;
        _transformation = Transformation.None;
        _sizing = Sizing.Empty;
    }

    [RelayCommand]
    private void PointerMove(Vector2 position)
    {
        if (_isPanning)
        {
            Translation = _panStartTranslation + (position - _panStartPointerPosition);
            WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
            return;
        }
        
        if (SelectedNodes.Count == 0) return;

        position = RelativePositionToDialog(position);

        int i = 0;
        foreach (var controlViewModel in SelectedControlViewModels)
        {
            var transformationStartRectangle = _transformationStartRectangles[i];
            switch (_transformation)
            {
                case Transformation.Size:
                    if (_sizing.Left)
                    {
                        position.X = Math.Min(position.X, transformationStartRectangle.Right);
                        controlViewModel.X = (int)position.X;
                        controlViewModel.Width =
                            transformationStartRectangle.Right - controlViewModel.X;
                    }

                    if (_sizing.Top)
                    {
                        position.Y = Math.Min(position.Y, transformationStartRectangle.Bottom);
                        controlViewModel.Y = (int)position.Y;
                        controlViewModel.Height =
                            transformationStartRectangle.Bottom - controlViewModel.Y;
                    }

                    if (_sizing.Right)
                    {
                        position.X = Math.Max(position.X, transformationStartRectangle.X);
                        controlViewModel.Width = (int)(transformationStartRectangle.Width +
                                                       (position.X - transformationStartRectangle.Right));
                    }

                    if (_sizing.Bottom)
                    {
                        position.Y = Math.Max(position.Y, transformationStartRectangle.Y);
                        controlViewModel.Height = (int)(transformationStartRectangle.Height +
                                                        (position.Y - transformationStartRectangle.Bottom));
                    }

                    break;
                case Transformation.Move:
                    controlViewModel.X =
                        (int)(transformationStartRectangle.X + (position.X - _transformationStartPointerPosition.X));
                    controlViewModel.Y =
                        (int)(transformationStartRectangle.Y + (position.Y - _transformationStartPointerPosition.Y));
                    break;
            }

            var processedRectangle =
                _dialogEditorSettingsViewModel.Positioner.Transform(DialogViewModel.Dialog.Root,
                    controlViewModel.Control);
            controlViewModel.X = processedRectangle.X;
            controlViewModel.Y = processedRectangle.Y;
            controlViewModel.Width = processedRectangle.Width;
            controlViewModel.Height = processedRectangle.Height;
            i++;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteSelectedNode()
    {
        // DialogViewModel.Dialog.Root.Children.Remove(SelectedNodes!);
        // SelectedNode = null;
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void BringSelectedNodeToFront()
    {
        // DialogViewModel.Dialog.Root.Children.Remove(SelectedNode!);
        // DialogViewModel.Dialog.Root.Children.Add(SelectedNode!);
        WeakReferenceMessenger.Default.Send(new CanvasInvalidationMessage(0));
    }

    [RelayCommand]
    private async Task SaveRc()
    {
        var rc = new RcDialogSerializer()
        {
            GenerateCompilable = true
        }.Serialize(
            new DefaultLayoutEngine().DoLayout(DialogViewModel.Dialog), DialogViewModel.Dialog);
        var header = new CxxHeaderInformationGenerator().Generate(DialogViewModel.Dialog);

        var resourceFile =
            await _filePickerService.TryPickSaveFileAsync("rsrc.rc", ("Resource File", new[] { "rc" }));
        var headerFile =
            await _filePickerService.TryPickSaveFileAsync("resource.h", ("C/C++ Header File", new[] { "h" }));

        if (resourceFile == null || headerFile == null) return;

        await File.WriteAllTextAsync(resourceFile, rc);
        await File.WriteAllTextAsync(headerFile, header);
    }
}
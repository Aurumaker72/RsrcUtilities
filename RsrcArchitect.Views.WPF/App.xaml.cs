using System.Windows;
using RsrcArchitect.Views.WPF.Services;

namespace RsrcArchitect.Views.WPF;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    internal static readonly FilesService FilesService = new FilesService();

}
using System.Reflection;
using RsrcArchitect.ViewModels.Controls;
using RsrcCore.Controls;

namespace RsrcArchitect.ViewModels.Factories;

internal static class ControlViewModelFactory
{
    private static Dictionary<string, Type> _typeCache = new();

    /// <summary>
    ///     Creates a class inheriting <see cref="ControlViewModel" /> which maps to the specified control
    /// </summary>
    /// <returns>An instance of a class inheriting <see cref="ControlViewModel" /></returns>
    internal static ControlViewModel Create(Control control, Func<string, bool> isIdentifierInUse)
    {
        var typeName = $"{control.GetType().Name}ViewModel";
        if (!_typeCache.TryGetValue(typeName, out var type))
        {
            _typeCache[typeName] = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(t => t.Name == typeName)!;
             type = _typeCache[typeName];
        }
        if (type == null)
        {
            throw new ArgumentException($"Couldn't map a control vm to {typeName}");
        }
        return (ControlViewModel)Activator.CreateInstance(type, control, isIdentifierInUse);
    }
}
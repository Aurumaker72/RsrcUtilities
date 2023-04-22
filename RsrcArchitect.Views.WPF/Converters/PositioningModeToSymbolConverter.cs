using System;
using System.Globalization;
using System.Windows.Data;
using RsrcArchitect.ViewModels.Types;
using Wpf.Ui.Common;

namespace RsrcArchitect.Views.WPF.Converters;

public class PositioningModeToSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PositioningModes positioningMode)
        {
            return positioningMode switch
            {
                PositioningModes.Freeform => SymbolRegular.Cursor24,
                PositioningModes.Grid => SymbolRegular.Grid24,
                PositioningModes.Snap => SymbolRegular.BrainCircuit24,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        throw new ArgumentException($"Expected value of type {nameof(PositioningModes)}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
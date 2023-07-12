using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using RsrcArchitect.ViewModels.Types;

namespace RsrcArchitect.Views.WPF.Converters;

public class EnumToCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Enum.GetValues(value.GetType())
               .Cast<Enum>()
               .ToList();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
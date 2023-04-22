using System.Windows;
using System.Windows.Media;

namespace RsrcArchitect.Views.WPF.Extensions;

public static class FrameworkElementExtensions
{
    public static T FindElementByName<T>(this FrameworkElement element, string sChildName) where T : FrameworkElement
    {
        T childElement = null;
        var nChildCount = VisualTreeHelper.GetChildrenCount(element);
        for (var i = 0; i < nChildCount; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

            if (child == null)
                continue;

            if (child is T && child.Name.Equals(sChildName))
            {
                childElement = (T)child;
                break;
            }

            childElement = FindElementByName<T>(child, sChildName);

            if (childElement != null)
                break;
        }

        return childElement;
    }
}
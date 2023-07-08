using Windows.Win32.Foundation;

namespace RsrcArchitect.Views.WPF.Extensions;

internal static class StringExtensions
{
    public static unsafe PCWSTR ToPcwstr(this string @string)
    {
        fixed (char* p = @string)
        {
            return p;
        }
    }
    
}
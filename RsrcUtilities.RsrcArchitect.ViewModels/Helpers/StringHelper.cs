namespace RsrcUtilities.RsrcArchitect.ViewModels.Helpers;

internal static class StringHelper
{
    public static string GetRandomAlphabeticString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }
}
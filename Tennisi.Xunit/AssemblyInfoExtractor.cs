﻿namespace Tennisi.Xunit;

internal static class AssemblyInfoExtractor
{
    private static readonly char[] Separator = { ',' };

    internal static string ExtractNameAndVersion(string input)
    {
        var parts = input.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        var namePart = string.Empty;
        var versionPart = string.Empty;

        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            if (trimmedPart.StartsWith("Version=", StringComparison.OrdinalIgnoreCase))
            {
                versionPart = trimmedPart["Version=".Length..].Trim();
            }
            else if (!trimmedPart.StartsWith("Culture=", StringComparison.OrdinalIgnoreCase) &&
                     !trimmedPart.StartsWith("PublicKeyToken=", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(namePart))
                {
                    namePart += ", ";
                }
                namePart += trimmedPart.Trim('"');
            }
        }

        return $"{namePart}, Version={versionPart}".TrimEnd(',', ' ');
    }
}
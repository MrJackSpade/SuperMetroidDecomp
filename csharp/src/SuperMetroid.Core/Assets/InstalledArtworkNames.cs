namespace SuperMetroid.Core.Assets;

/// <summary>Turns an identity's name into the lower-case, hyphenated form installed file names use.</summary>
internal static class InstalledArtworkNames
{
    /// <summary>The hyphenated form of a PascalCase name: <c>RedBrinstarKraid</c> becomes <c>red-brinstar-kraid</c>.</summary>
    internal static string Hyphenated(string name)
    {
        var text = new System.Text.StringBuilder(name.Length + 8);
        for (int index = 0; index < name.Length; index++)
        {
            char letter = name[index];
            if (char.IsUpper(letter) && index > 0)
                text.Append('-');
            text.Append(char.ToLowerInvariant(letter));
        }
        return text.ToString();
    }
}

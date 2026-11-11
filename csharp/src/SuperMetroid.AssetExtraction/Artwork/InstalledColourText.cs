using System.Globalization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// How an installed sheet manifest writes a colour (#1633): <c>#rrggbb</c>, each channel two lowercase hexadecimal digits
/// of its eight-bit value, as an editor shows it.
/// </summary>
internal static class InstalledColourText
{
    /// <summary>The characters of one written colour: the mark and two digits per channel.</summary>
    private const int Length = 7;

    /// <summary><paramref name="colour"/> as the manifest writes it.</summary>
    internal static string Format(RgbColor colour) =>
        string.Create(CultureInfo.InvariantCulture, $"#{colour.Red:x2}{colour.Green:x2}{colour.Blue:x2}");

    /// <summary>The colour <paramref name="text"/> writes.</summary>
    /// <param name="text">The written colour.</param>
    /// <param name="context">Names what the colour belongs to in a failure.</param>
    /// <exception cref="InvalidDataException">The text is not a colour written as <c>#rrggbb</c>.</exception>
    internal static RgbColor Parse(string? text, string context)
    {
        if (text is not { Length: Length } || text[0] != '#' ||
            !byte.TryParse(text.AsSpan(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte red) ||
            !byte.TryParse(text.AsSpan(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte green) ||
            !byte.TryParse(text.AsSpan(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte blue))
        {
            throw new InvalidDataException($"{Ensure.NotNull(context)} names colour \"{text}\", not one written as #rrggbb.");
        }
        return new RgbColor(red, green, blue);
    }
}

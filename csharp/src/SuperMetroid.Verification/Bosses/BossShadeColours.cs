using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// The colours the import's boss shades show (#1512), laid in the cartridge's colour order for the checks that compare
/// them with the cartridge: a room picture's inks are the first hundred and twenty-eight colours and a sprite sheet's
/// ink is its colour less a hundred and twenty-eight.
/// </summary>
internal static class BossShadeColours
{
    /// <summary>The colour a room picture's ink zero stands at.</summary>
    internal const int Room = 0;

    /// <summary>The colour a sprite sheet's ink zero stands at.</summary>
    internal const int Sprites = 128;

    /// <summary>The number of colours the cartridge holds.</summary>
    private const int ColourCount = 256;

    /// <summary>Every colour as <paramref name="word"/>, the colours under the shades a check lays.</summary>
    internal static ushort[] Filled(ushort word) => [.. Enumerable.Repeat(word, ColourCount)];

    /// <summary>Lays <paramref name="shade"/>, as the import made it among <paramref name="made"/>, over <paramref name="colours"/> from <paramref name="offset"/> on.</summary>
    /// <exception cref="InvalidOperationException">The import did not make the shade once.</exception>
    internal static ushort[] Lay(ushort[] colours, IReadOnlyList<SheetShade> made, PictureShade shade, int offset)
    {
        ArgumentNullException.ThrowIfNull(colours);
        SheetShade found = made.Single(candidate => candidate.Shade == shade);
        for (int index = 0; index < found.Inks.Length; index++)
            colours[offset + found.Inks[index]] = found.Colours[index].ToWord();
        return colours;
    }

    /// <summary>The colours <paramref name="shades"/> laid in order over <paramref name="under"/> show, from <paramref name="made"/>.</summary>
    internal static ushort[] Laid(IEnumerable<PictureShade> shades, IReadOnlyList<SheetShade> made, int offset, ushort under)
    {
        ArgumentNullException.ThrowIfNull(shades);
        ushort[] colours = Filled(under);
        foreach (PictureShade shade in shades)
            Lay(colours, made, shade, offset);
        return colours;
    }
}

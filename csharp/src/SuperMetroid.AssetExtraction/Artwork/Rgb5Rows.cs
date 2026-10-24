using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Validates and packs rows of editable RGB5 colours, as every colour catalog stores them.</summary>
internal static class Rgb5Rows
{
    /// <summary>Checks a row's length and five-bit channels before packing its SNES colour words.</summary>
    /// <param name="source">The authored row.</param>
    /// <param name="count">The colours the row must hold.</param>
    /// <param name="label">What the row is, naming it in a rejection.</param>
    /// <exception cref="InvalidDataException">The row is missing, has the wrong length, or holds a channel outside 0..31.</exception>
    internal static Bgr555[] Compile(PaletteRgb5[]? source, int count, string label)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"{label} requires {count} RGB5 colors.");
        var compiled = new Bgr555[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"{label} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToBgr555();
        }
        return compiled;
    }
}

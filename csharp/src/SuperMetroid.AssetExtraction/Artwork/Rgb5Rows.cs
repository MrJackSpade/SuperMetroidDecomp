using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Validates and packs rows of editable RGB5 colours, as every colour catalog stores them.</summary>
internal static class Rgb5Rows
{
    /// <summary>Checks a row's length and five-bit channels before packing its SNES colour words.</summary>
    /// <param name="source">The authored row.</param>
    /// <param name="count">The colours the row must hold.</param>
    /// <param name="label">What the row is, naming it in a rejection.</param>
    /// <exception cref="InvalidDataException">The row is missing, has the wrong length, or holds a channel outside 0..31.</exception>
    internal static RgbColor[] Compile(PaletteRgb5[]? source, int count, string label)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"{label} requires {count} RGB5 colors.");
        var compiled = new RgbColor[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"{label} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToRgbColor();
        }
        return compiled;
    }

    /// <summary>
    /// Checks rows keyed by each identity's installed key (#2565), one for each of <paramref name="identities"/>, and packs
    /// each identity's colours.
    /// </summary>
    /// <param name="source">The authored rows, keyed by installed key.</param>
    /// <param name="identities">The identities the rows colour.</param>
    /// <param name="key">An identity's installed key.</param>
    /// <param name="count">The colours each row must hold.</param>
    /// <param name="label">What the rows are, naming them in a rejection.</param>
    /// <exception cref="InvalidDataException">An identity has no row, an unknown row is held, or a row is invalid.</exception>
    internal static Dictionary<T, RgbColor[]> CompileKeyed<T>(Dictionary<string, PaletteRgb5[]>? source, IReadOnlyList<T> identities,
        Func<T, string> key, int count, string label)
        where T : notnull
    {
        if (source is null || source.Count != identities.Count)
            throw new InvalidDataException($"{label} requires {identities.Count} rows, each keyed by what it colours.");
        return identities.ToDictionary(identity => identity, identity => Compile(
            source.TryGetValue(key(identity), out PaletteRgb5[]? row) ? row : throw new InvalidDataException($"{label} hold no row for {key(identity)}."),
            count, $"{label} {key(identity)}"));
    }

    /// <summary>
    /// The rows of a cartridge table that holds one row for each identity in <paramref name="tableOrder"/>'s order, keyed by
    /// each identity's installed key (#2565).
    /// </summary>
    /// <param name="tableOrder">The identities in the order the cartridge's table holds their rows.</param>
    /// <param name="key">An identity's installed key.</param>
    /// <param name="row">The table's row at a place.</param>
    internal static Dictionary<string, PaletteRgb5[]> KeyedInTableOrder<T>(IReadOnlyList<T> tableOrder, Func<T, string> key, Func<int, PaletteRgb5[]> row) =>
        tableOrder.Select((identity, place) => KeyValuePair.Create(key(identity), row(place))).ToDictionary(StringComparer.Ordinal);
}

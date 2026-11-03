using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

/// <summary>
/// One sixteen-color Kraid source stored as its supplied deviations from the stock paint in
/// <see cref="KraidPaint"/>; an unedited stock band stores nothing.
/// </summary>
internal sealed class KraidStockBand : IReadOnlyList<RgbColor>
{
    /// <summary>Named source that supplies calculated unedited colors.</summary>
    private readonly KraidPaletteSource source;
    /// <summary>Only supplied colors that differ from the reviewed stock paint.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> deviations;

    /// <summary>Keeps the independent edits to one sixteen-color source.</summary>
    /// <param name="source">The named source whose stock paint unedited colors calculate.</param>
    /// <param name="deviations">The supplied colors the import found differing from the stock paint.</param>
    internal KraidStockBand(KraidPaletteSource source, Dictionary<PaintColour, RgbColor> deviations)
    {
        this.source = source;
        this.deviations = deviations;
    }

    /// <summary>Color count of this named source.</summary>
    public int Count => KraidPalettes.ColorCount(source);
    /// <summary>Edited color at an index, or its calculated stock paint.</summary>
    public RgbColor this[int index] => deviations.TryGetValue(new PaintColour(0, index), out RgbColor supplied)
        ? supplied : KraidPaint.Color(source, index);

    /// <summary>Visits source colors in their palette order.</summary>
    public IEnumerator<RgbColor> GetEnumerator()
    {
        for (int index = 0; index < Count; index++)
            yield return this[index];
    }
    /// <summary>Exposes the same palette order to non-generic enumeration.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// One sixteen-color Kraid source stored as its supplied deviations from the stock paint in
/// <see cref="KraidPaint"/>; an unedited stock band stores nothing.
/// </summary>
internal sealed class KraidStockBand : IReadOnlyList<RgbColor>
{
    /// <summary>Named native source that supplies calculated unedited colors.</summary>
    private readonly KraidPaletteSource source;
    /// <summary>Only supplied colors that differ from the reviewed stock paint.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> deviations = new();

    /// <summary>Records independent edits to one sixteen-color source.</summary>
    internal KraidStockBand(KraidPaletteSource source, RgbColor[] supplied)
    {
        this.source = source;
        for (int index = 0; index < supplied.Length; index++)
            if (KraidPaint.Color(source, index) != supplied[index])
                deviations.Add(new ColourCell(0, index), supplied[index]);
    }

    /// <summary>Native color count of this named source.</summary>
    public int Count => KraidPalettes.ColorCount(source);
    /// <summary>Edited color at an index, or its calculated stock paint.</summary>
    public RgbColor this[int index] => deviations.TryGetValue(new ColourCell(0, index), out RgbColor supplied)
        ? supplied : KraidPaint.Color(source, index);

    /// <summary>Visits source colors in their native palette order.</summary>
    public IEnumerator<RgbColor> GetEnumerator()
    {
        for (int index = 0; index < Count; index++)
            yield return this[index];
    }
    /// <summary>Exposes the same palette order to non-generic enumeration.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

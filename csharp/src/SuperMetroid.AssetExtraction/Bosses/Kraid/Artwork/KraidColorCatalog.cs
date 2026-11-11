using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

/// <summary>Kraid's installed RGB5 sources, each kept as installed; boss phase and fade arithmetic stay engine-owned.</summary>
public sealed class KraidColorCatalog
{
    /// <summary>
    /// Each named editable source: the room-backdrop fade targets, the rock sprite targets staged during initial setup,
    /// the white hurt flash and eight body health bands, the independently editable sprite flash and health bands, and
    /// the arm and body death colours installed before the fade.
    /// </summary>
    internal readonly IReadOnlyDictionary<KraidPaletteSource, IReadOnlyList<RgbColor>> sources;

    /// <summary>Keeps every named source's complete installed colours.</summary>
    /// <param name="sources">Each of the five named sources' colours, as installed.</param>
    /// <exception cref="InvalidDataException">A source is missing or holds another number of colours.</exception>
    internal KraidColorCatalog(IReadOnlyDictionary<KraidPaletteSource, RgbColor[]> sources)
    {
        var complete = new Dictionary<KraidPaletteSource, IReadOnlyList<RgbColor>>();
        foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
        {
            int count = KraidPalettes.ColorCount(source);
            if (!sources.TryGetValue(source, out RgbColor[]? colours) || colours.Length != count)
                throw new InvalidDataException($"Kraid {source} requires {count} installed colors.");
            complete.Add(source, (RgbColor[])colours.Clone());
        }
        this.sources = complete;
    }

    /// <summary>
    /// Selects one of five named editable sources directly. Each source keeps its
    /// own loaded color bounds; unknown sources and out-of-range indices are rejected.
    /// </summary>
    public RgbColor Resolve(KraidPaletteSource source, int index) => KraidColors.Resolve(this, source, index);

    /// <summary>The eyes' colours under the hurt flash and each health band, which gameplay's eye glow reads back (#1512).</summary>
    internal KraidEyeColours Eyes() => KraidColors.Eyes(this);
}

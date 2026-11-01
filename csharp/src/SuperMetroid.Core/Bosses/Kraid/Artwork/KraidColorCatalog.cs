using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Editable Kraid RGB5 sources; boss phase and fade arithmetic stay engine-owned.</summary>
public sealed class KraidColorCatalog
{
    /// <summary>Identity of the selected colours, computed from them rather than kept with the game state.</summary>
    public string ContentIdentity => KraidColors.ContentIdentity(this);

    /// <summary>
    /// Each named editable source: the room-backdrop fade targets, the rock sprite targets staged during initial setup,
    /// the white hurt flash and eight body health bands, the independently editable sprite flash and health bands, and
    /// the arm and body death colours installed before the fade.
    /// </summary>
    internal readonly IReadOnlyDictionary<KraidPaletteSource, IReadOnlyList<RgbColor>> sources;

    /// <summary>Retains the compiled sources, the secondary bands sharing the health bands' storage when they match.</summary>
    /// <param name="sources">Each named source as the import compiled it.</param>
    internal KraidColorCatalog(IReadOnlyDictionary<KraidPaletteSource, IReadOnlyList<RgbColor>> sources) => this.sources = sources;

    /// <summary>
    /// Selects one of five named editable sources directly. Each source keeps its
    /// own loaded color bounds; unknown sources and out-of-range indices are rejected.
    /// </summary>
    public RgbColor Resolve(KraidPaletteSource source, int index) => KraidColors.Resolve(this, source, index);
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Editable Kraid RGB5 sources; boss phase and fade arithmetic stay engine-owned.</summary>
public sealed class KraidColorCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => KraidColors.ContentIdentity(this);

    /// <summary>
    /// Each named editable source: the room-backdrop fade targets, the rock sprite targets staged during initial setup,
    /// the white hurt flash and eight body health bands, the independently editable sprite flash and health bands, and
    /// the arm and body death colours installed before the fade.
    /// </summary>
    internal readonly IReadOnlyDictionary<KraidPaletteSource, IReadOnlyList<RgbColor>> sources;

    /// <summary>Compiles five authored sources, sharing health-band storage only when secondary colors match exactly.</summary>
    internal KraidColorCatalog(RgbColor[] roomBackdrop, RgbColor[] initialTarget, RgbColor[] health,
        RgbColor[] secondary, RgbColor[] deathArm)
        : this(new KraidStockBand(KraidPaletteSource.RoomBackdrop, roomBackdrop),
            new KraidStockBand(KraidPaletteSource.InitialTarget, initialTarget),
            new KraidHealthBands(health), secondary,
            new KraidStockBand(KraidPaletteSource.DeathArm, deathArm))
    {
    }

    /// <summary>Retains the compiled sources, sharing the health bands as the secondary bands when they match.</summary>
    private KraidColorCatalog(KraidStockBand roomBackdrop, KraidStockBand initialTarget, KraidHealthBands health,
        RgbColor[] secondary, KraidStockBand deathArm) =>
        sources = new Dictionary<KraidPaletteSource, IReadOnlyList<RgbColor>>
        {
            [KraidPaletteSource.RoomBackdrop] = roomBackdrop,
            [KraidPaletteSource.InitialTarget] = initialTarget,
            [KraidPaletteSource.Health] = health,
            [KraidPaletteSource.Secondary] = KraidHealthBands.Secondary(secondary, health),
            [KraidPaletteSource.DeathArm] = deathArm,
        };

    /// <summary>
    /// Selects one of five named editable sources directly. Each source keeps its
    /// own loaded color bounds; unknown sources and out-of-range indices are rejected.
    /// </summary>
    public RgbColor Resolve(KraidPaletteSource source, int index) => KraidColors.Resolve(this, source, index);
}

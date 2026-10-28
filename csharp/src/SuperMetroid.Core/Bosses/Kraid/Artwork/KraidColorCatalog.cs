using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Editable Kraid RGB5 sources; boss phase and fade arithmetic stay engine-owned.</summary>
public sealed class KraidColorCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => KraidColors.ContentIdentity(this);

    /// <summary>Target colors for Kraid's room-backdrop fade.</summary>
    internal readonly IReadOnlyList<RgbColor> roomBackdrop;
    /// <summary>Rock sprite target colors staged during initial setup.</summary>
    internal readonly IReadOnlyList<RgbColor> initialTarget;
    /// <summary>White hurt flash and eight body health bands.</summary>
    internal readonly KraidHealthBands health;
    /// <summary>Independently editable sprite flash and health bands.</summary>
    internal readonly IReadOnlyList<RgbColor> secondary;
    /// <summary>Arm and body death colors installed before the fade.</summary>
    internal readonly IReadOnlyList<RgbColor> deathArm;

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
        (this.roomBackdrop, this.initialTarget, this.health, this.secondary, this.deathArm) =
            (roomBackdrop, initialTarget, health, KraidHealthBands.Secondary(secondary, health), deathArm);

    /// <summary>
    /// Selects one of five named editable sources directly. Each source keeps its
    /// own loaded color bounds; unknown sources and out-of-range indices are rejected.
    /// </summary>
    public RgbColor Resolve(KraidPaletteSource source, int index) => KraidColors.Resolve(this, source, index);
}

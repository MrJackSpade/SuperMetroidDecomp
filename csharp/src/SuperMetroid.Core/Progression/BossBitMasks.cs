namespace SuperMetroid.Core.Progression;

/// <summary>
/// The three boss-state bits whose retail meanings are proven. The remaining five bits in each saved area byte remain
/// losslessly preserved by raw SRAM APIs, but they cannot silently enter named gameplay logic (<see cref="ProvenBossBits"/>).
/// </summary>
public static class BossBitMasks
{
    /// <summary>Every currently proven flag in the cartridge's per-area boss byte.</summary>
    public const BossBits AllKnown =
        BossBits.AreaBoss | BossBits.AreaMiniBoss | BossBits.AreaTorizo;
}

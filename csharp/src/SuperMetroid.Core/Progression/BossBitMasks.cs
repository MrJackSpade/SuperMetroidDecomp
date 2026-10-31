namespace SuperMetroid.Core.Progression;

/// <summary>
/// The three boss flags whose retail meanings are proven. No routine of the game tests any other flag of an area's boss
/// state, so progress holds only these (<see cref="ProvenBossBits"/>).
/// </summary>
public static class BossBitMasks
{
    /// <summary>Every currently proven flag of an area's boss state.</summary>
    public const BossBits AllKnown =
        BossBits.AreaBoss | BossBits.AreaMiniBoss | BossBits.AreaTorizo;

    /// <summary>
    /// The proven flags in the order an area map's boss markers consume them: the first listed marker shows the area
    /// boss, the next the mini-boss, the next the Torizo.
    /// </summary>
    public static IReadOnlyList<BossBits> InMarkerOrder { get; } = [BossBits.AreaBoss, BossBits.AreaMiniBoss, BossBits.AreaTorizo];
}

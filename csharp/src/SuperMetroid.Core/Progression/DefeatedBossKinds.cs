namespace SuperMetroid.Core.Progression;

/// <summary>
/// The three kinds of boss an area records as defeated, whose retail meanings are proven. Nothing in the game asks for any
/// other, so progress holds only these (<see cref="ProvenDefeatedBosses"/>).
/// </summary>
public static class DefeatedBossKinds
{
    /// <summary>Every kind of boss an area records.</summary>
    public const DefeatedBosses AllKnown =
        DefeatedBosses.AreaBoss | DefeatedBosses.AreaMiniBoss | DefeatedBosses.AreaTorizo;

    /// <summary>
    /// The kinds in the order an area map's boss markers show them: the first listed marker shows the area boss, the next
    /// the mini-boss, the next the Torizo.
    /// </summary>
    public static IReadOnlyList<DefeatedBosses> InMarkerOrder { get; } =
        [DefeatedBosses.AreaBoss, DefeatedBosses.AreaMiniBoss, DefeatedBosses.AreaTorizo];
}

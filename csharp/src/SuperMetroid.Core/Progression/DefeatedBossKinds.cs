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

    /// <summary>Every kind of boss an area records, listed area boss first, then the mini-boss, then the Torizo.</summary>
    public static IReadOnlyList<DefeatedBosses> Listed { get; } =
        [DefeatedBosses.AreaBoss, DefeatedBosses.AreaMiniBoss, DefeatedBosses.AreaTorizo];
}

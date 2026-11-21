namespace SuperMetroid.Core.Progression;

/// <summary>Which of its area's defeats each boss's defeat is recorded as (#2563).</summary>
public static class AreaBossDefinitions
{
    /// <summary>
    /// The defeat each boss is recorded as in its area's progress: the area's boss for Kraid, both Ridleys, Phantoon,
    /// Draygon and Mother Brain, its mini-boss for Spore Spawn, Crocomire and Botwoon, and its Torizo for the Torizos.
    /// </summary>
    public static IReadOnlyDictionary<AreaBoss, DefeatedBosses> RecordedAs { get; } = new Dictionary<AreaBoss, DefeatedBosses>
    {
        [AreaBoss.CeresRidley] = DefeatedBosses.AreaBoss,
        [AreaBoss.Torizo] = DefeatedBosses.AreaTorizo,
        [AreaBoss.Kraid] = DefeatedBosses.AreaBoss,
        [AreaBoss.SporeSpawn] = DefeatedBosses.AreaMiniBoss,
        [AreaBoss.Ridley] = DefeatedBosses.AreaBoss,
        [AreaBoss.Crocomire] = DefeatedBosses.AreaMiniBoss,
        [AreaBoss.Phantoon] = DefeatedBosses.AreaBoss,
        [AreaBoss.Draygon] = DefeatedBosses.AreaBoss,
        [AreaBoss.Botwoon] = DefeatedBosses.AreaMiniBoss,
        [AreaBoss.MotherBrain] = DefeatedBosses.AreaBoss,
    };
}

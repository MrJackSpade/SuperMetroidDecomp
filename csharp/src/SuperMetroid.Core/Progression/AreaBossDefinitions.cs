namespace SuperMetroid.Core.Progression;

/// <summary>Each area boss (<see cref="AreaBoss"/>).</summary>
internal static class AreaBossDefinitions
{
    /// <summary>
    /// Each boss: X-ray keeps the room's background while Kraid, Crocomire, Phantoon, Draygon or Mother Brain is loaded; the
    /// defeat each is recorded as in its area's progress is the area's boss for Kraid, both Ridleys, Phantoon, Draygon and
    /// Mother Brain, its mini-boss for Spore Spawn, Crocomire and Botwoon, and its Torizo for the Torizos (#2563).
    /// </summary>
    internal static readonly IReadOnlyDictionary<AreaBoss, AreaBossDefinition> Bosses = new Dictionary<AreaBoss, AreaBossDefinition>
    {
        [AreaBoss.CeresRidley] = new(XrayKeepsBackground: false, RecordedAs: DefeatedBosses.AreaBoss),
        [AreaBoss.Torizo] = new(XrayKeepsBackground: false, RecordedAs: DefeatedBosses.AreaTorizo),
        [AreaBoss.Kraid] = new(XrayKeepsBackground: true, RecordedAs: DefeatedBosses.AreaBoss),
        [AreaBoss.SporeSpawn] = new(XrayKeepsBackground: false, RecordedAs: DefeatedBosses.AreaMiniBoss),
        [AreaBoss.Ridley] = new(XrayKeepsBackground: false, RecordedAs: DefeatedBosses.AreaBoss),
        [AreaBoss.Crocomire] = new(XrayKeepsBackground: true, RecordedAs: DefeatedBosses.AreaMiniBoss),
        [AreaBoss.Phantoon] = new(XrayKeepsBackground: true, RecordedAs: DefeatedBosses.AreaBoss),
        [AreaBoss.Draygon] = new(XrayKeepsBackground: true, RecordedAs: DefeatedBosses.AreaBoss),
        [AreaBoss.Botwoon] = new(XrayKeepsBackground: false, RecordedAs: DefeatedBosses.AreaMiniBoss),
        [AreaBoss.MotherBrain] = new(XrayKeepsBackground: true, RecordedAs: DefeatedBosses.AreaBoss),
    };
}

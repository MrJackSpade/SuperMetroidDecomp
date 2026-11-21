namespace SuperMetroid.Core.Progression;

/// <summary>Each area boss (<see cref="AreaBoss"/>).</summary>
internal static class AreaBossDefinitions
{
    /// <summary>Each boss: X-ray keeps the room's background while Kraid, Crocomire, Phantoon, Draygon or Mother Brain is loaded.</summary>
    internal static readonly IdentityTable<AreaBoss, AreaBossDefinition> Bosses = new(new Dictionary<AreaBoss, AreaBossDefinition>
    {
        [AreaBoss.CeresRidley] = new(XrayKeepsBackground: false),
        [AreaBoss.Torizo] = new(XrayKeepsBackground: false),
        [AreaBoss.Kraid] = new(XrayKeepsBackground: true),
        [AreaBoss.SporeSpawn] = new(XrayKeepsBackground: false),
        [AreaBoss.Ridley] = new(XrayKeepsBackground: false),
        [AreaBoss.Crocomire] = new(XrayKeepsBackground: true),
        [AreaBoss.Phantoon] = new(XrayKeepsBackground: true),
        [AreaBoss.Draygon] = new(XrayKeepsBackground: true),
        [AreaBoss.Botwoon] = new(XrayKeepsBackground: false),
        [AreaBoss.MotherBrain] = new(XrayKeepsBackground: true),
    });
}

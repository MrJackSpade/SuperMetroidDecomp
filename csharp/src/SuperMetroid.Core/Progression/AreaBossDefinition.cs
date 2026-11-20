namespace SuperMetroid.Core.Progression;

/// <summary>What one area boss is (<see cref="AreaBossDefinitions.Bosses"/>).</summary>
/// <param name="XrayKeepsBackground">
/// Whether X-ray keeps the room's background while the boss is loaded rather than revealing the room's blocks (#2485).
/// </param>
internal sealed record AreaBossDefinition(bool XrayKeepsBackground);

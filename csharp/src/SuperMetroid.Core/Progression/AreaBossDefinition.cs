namespace SuperMetroid.Core.Progression;

/// <summary>What one area boss is (<see cref="AreaBossDefinitions.Bosses"/>).</summary>
/// <param name="XrayKeepsBackground">
/// Whether X-ray keeps the room's background while the boss is loaded rather than revealing the room's blocks (#2485).
/// </param>
/// <param name="RecordedAs">Which of its area's defeats the boss's defeat is recorded as in the area's progress (#2563).</param>
[IndependentFlag("whether X-ray keeps the background while the boss is loaded is a fact of that boss alone")]
internal sealed record AreaBossDefinition(bool XrayKeepsBackground, DefeatedBosses RecordedAs);

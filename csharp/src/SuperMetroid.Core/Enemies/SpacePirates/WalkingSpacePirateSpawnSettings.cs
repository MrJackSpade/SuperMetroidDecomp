namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a walking space pirate patrols (#1472, #1519).</summary>
/// <param name="Options">Its options.</param>
/// <param name="PatrolReach">The pixels it patrols either side of its placement.</param>
internal sealed record WalkingSpacePirateSpawnSettings(WalkingSpacePirateOptions Options, int PatrolReach) : EnemySpawnSettings;

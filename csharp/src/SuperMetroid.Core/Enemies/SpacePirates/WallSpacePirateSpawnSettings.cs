namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a wall space pirate jumps (#1472, #1519).</summary>
/// <param name="Options">Its options.</param>
/// <param name="JumpSpan">The pixels between its walls.</param>
internal sealed record WallSpacePirateSpawnSettings(WallSpacePirateOptions Options, int JumpSpan) : EnemySpawnSettings;

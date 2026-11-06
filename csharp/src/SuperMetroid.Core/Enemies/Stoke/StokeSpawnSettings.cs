namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Which way a Stoke moves and how fast (#1472, #1519).</summary>
/// <param name="Direction">The way it moves.</param>
/// <param name="Speed">Its patrol speed in pixels per update; it patrols equally fast either way.</param>
internal sealed record StokeSpawnSettings(StokeDirection Direction, SignedSixteenSixteen Speed) : EnemySpawnSettings;

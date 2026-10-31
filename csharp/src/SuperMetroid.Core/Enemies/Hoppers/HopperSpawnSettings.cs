namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Whether a hopper starts on the ceiling (#1472).</summary>
/// <param name="UpsideDown">Whether it starts on the ceiling.</param>
internal sealed record HopperSpawnSettings(bool UpsideDown) : EnemySpawnSettings;

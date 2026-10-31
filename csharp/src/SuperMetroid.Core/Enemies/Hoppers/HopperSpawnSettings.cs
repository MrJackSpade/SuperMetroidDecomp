namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Whether a hopper starts on the ceiling (#1472).</summary>
/// <param name="Mounting">Whether it starts on the floor or the ceiling.</param>
internal sealed record HopperSpawnSettings(HopperMounting Mounting) : EnemySpawnSettings;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Whether a Dragon placement is the body or its wing (#1472).</summary>
/// <param name="Part">Which part of the dragon it is.</param>
internal sealed record DragonSpawnSettings(DragonPart Part) : EnemySpawnSettings;

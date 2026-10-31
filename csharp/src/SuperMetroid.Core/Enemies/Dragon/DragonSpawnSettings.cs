namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Whether a Dragon placement is the body or its wing (#1472).</summary>
/// <param name="IsWing">Whether it is the wing.</param>
internal sealed record DragonSpawnSettings(bool IsWing) : EnemySpawnSettings;

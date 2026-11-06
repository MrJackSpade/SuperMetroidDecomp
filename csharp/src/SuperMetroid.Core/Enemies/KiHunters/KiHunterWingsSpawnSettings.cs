namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>When a Ki-Hunter loses its wings (#1472, #1519).</summary>
/// <param name="DetachHealth">The body health at or below which the wings come off.</param>
internal sealed record KiHunterWingsSpawnSettings(int DetachHealth) : EnemySpawnSettings;

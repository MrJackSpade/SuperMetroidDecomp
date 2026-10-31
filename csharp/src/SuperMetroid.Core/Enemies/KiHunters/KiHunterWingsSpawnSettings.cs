namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>When a Ki-Hunter loses its wings (#1472).</summary>
/// <param name="DetachHealth">The body health, 0 through 65535, at or below which the wings come off.</param>
internal sealed record KiHunterWingsSpawnSettings(int DetachHealth) : EnemySpawnSettings
{
    /// <summary>The body health, 0 through 65535, at or below which the wings come off.</summary>
    public int DetachHealth { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, DetachHealth);
}

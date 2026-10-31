namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>How a Kzan falls (#1472).</summary>
/// <param name="FallSpeedIndex">Its fall speed, 0 through 255.</param>
/// <param name="FallWait">Updates, 0 through 255, it waits between falls.</param>
/// <param name="FallDistance">Pixels, 0 through 255, it falls.</param>
internal sealed record KzanSpawnSettings(int FallSpeedIndex, int FallWait, int FallDistance) : EnemySpawnSettings
{
    /// <summary>Its fall speed, 0 through 255.</summary>
    public int FallSpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, FallSpeedIndex);

    /// <summary>Updates, 0 through 255, it waits between falls.</summary>
    public int FallWait { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, FallWait);

    /// <summary>Pixels, 0 through 255, it falls.</summary>
    public int FallDistance { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, FallDistance);
}

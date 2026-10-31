namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>How a moving platform travels (#1472).</summary>
/// <param name="Movement">The way it moves.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, it moves at.</param>
/// <param name="MaximumYSpeedIndex">Its maximum fall speed, 0 through 255.</param>
internal sealed record PlatformSpawnSettings(PlatformHorizontalMovement Movement, int SpeedIndex, int MaximumYSpeedIndex) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 255, it moves at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>Its maximum fall speed, 0 through 255.</summary>
    public int MaximumYSpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, MaximumYSpeedIndex);
}

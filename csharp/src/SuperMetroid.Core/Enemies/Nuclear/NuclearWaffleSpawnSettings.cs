namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>How a nuclear waffle orbits (#1472).</summary>
/// <param name="AngularSpeedIndex">Its angular speed, 0 through 255.</param>
/// <param name="OrbitRadius">Its orbit radius, 0 through 255 pixels.</param>
/// <param name="Direction">Its orbit direction, 0 through 255.</param>
/// <param name="WaitTime">Updates, 0 through 255, it waits.</param>
internal sealed record NuclearWaffleSpawnSettings(int AngularSpeedIndex, int OrbitRadius, int Direction, int WaitTime) : EnemySpawnSettings
{
    /// <summary>Its angular speed, 0 through 255.</summary>
    public int AngularSpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, AngularSpeedIndex);

    /// <summary>Its orbit radius, 0 through 255 pixels.</summary>
    public int OrbitRadius { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, OrbitRadius);

    /// <summary>Its orbit direction, 0 through 255.</summary>
    public int Direction { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Direction);

    /// <summary>Updates, 0 through 255, it waits.</summary>
    public int WaitTime { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, WaitTime);
}

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>How a Fireflea flies (#1472).</summary>
/// <param name="Circular">Whether it circles rather than bobs vertically.</param>
/// <param name="PositiveSpeed">Whether it takes its speed record's positive speed.</param>
/// <param name="InitialAngle">Its first angle on its circle, 0 through 255.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, it flies at.</param>
/// <param name="RadiusIndex">Its radius, 0 through 255.</param>
internal sealed record FirefleaSpawnSettings(bool Circular, bool PositiveSpeed, int InitialAngle, int SpeedIndex, int RadiusIndex) : EnemySpawnSettings
{
    /// <summary>Its first angle on its circle, 0 through 255.</summary>
    public int InitialAngle { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, InitialAngle);

    /// <summary>The common linear-speed record, 0 through 255, it flies at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>Its radius, 0 through 255.</summary>
    public int RadiusIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, RadiusIndex);
}

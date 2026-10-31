namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Whether an Evir placement is its body or its arms, and how the body bobs (#1472).</summary>
/// <param name="Part">Which part of the Evir it is.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, the body bobs at.</param>
/// <param name="DirectionChangePeriod">Updates, 0 through 255, between the body's direction changes.</param>
internal sealed record EvirSpawnSettings(EvirBodyPart Part, int SpeedIndex, int DirectionChangePeriod) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 255, the body bobs at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>Updates, 0 through 255, between the body's direction changes.</summary>
    public int DirectionChangePeriod { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, DirectionChangePeriod);
}

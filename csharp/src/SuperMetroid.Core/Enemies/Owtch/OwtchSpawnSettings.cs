namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>How an Owtch patrols (#1472).</summary>
/// <param name="InitialState">Its first state, 0 through 255.</param>
/// <param name="UndergroundTimerIndex">Its underground time, 0 through 255.</param>
/// <param name="SpeedIndex">Its speed, 0 through 255.</param>
/// <param name="DistanceIndex">Its travel distance, 0 through 255.</param>
internal sealed record OwtchSpawnSettings(int InitialState, int UndergroundTimerIndex, int SpeedIndex, int DistanceIndex) : EnemySpawnSettings
{
    /// <summary>Its first state, 0 through 255.</summary>
    public int InitialState { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, InitialState);

    /// <summary>Its underground time, 0 through 255.</summary>
    public int UndergroundTimerIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, UndergroundTimerIndex);

    /// <summary>Its speed, 0 through 255.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>Its travel distance, 0 through 255.</summary>
    public int DistanceIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, DistanceIndex);
}

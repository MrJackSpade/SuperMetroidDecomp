namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>How a Bull speeds up and how fast it goes (#1472).</summary>
/// <param name="IntervalsIndex">Its acceleration and deceleration intervals, 0 through 65535.</param>
/// <param name="MaximumSpeedIndex">Its maximum speed, 0 through 65535.</param>
internal sealed record BullSpawnSettings(int IntervalsIndex, int MaximumSpeedIndex) : EnemySpawnSettings
{
    /// <summary>Its acceleration and deceleration intervals, 0 through 65535.</summary>
    public int IntervalsIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, IntervalsIndex);

    /// <summary>Its maximum speed, 0 through 65535.</summary>
    public int MaximumSpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, MaximumSpeedIndex);
}

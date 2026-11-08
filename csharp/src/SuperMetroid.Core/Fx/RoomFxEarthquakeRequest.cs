namespace SuperMetroid.Core.Fx;

/// <summary>One room-effect request for the global room-shake type and timer bits.</summary>
/// <param name="Type">The room-shake type to install, not negative.</param>
/// <param name="TimerBits">Frame flags merged into the running shake timer, within the timer's longest start.</param>
public readonly record struct RoomFxEarthquakeRequest(
    int Type,
    int TimerBits)
{
    /// <summary>The room-shake type to install; a negative type is rejected.</summary>
    public int Type { get; } = Ensure.AtLeastZero(Type);

    /// <summary>Frame flags merged into the running shake timer; flags past the timer's longest start are rejected.</summary>
    public int TimerBits { get; } = Ensure.BetweenInclusive(0, WrappingCountdown.LongestStart, TimerBits);
}

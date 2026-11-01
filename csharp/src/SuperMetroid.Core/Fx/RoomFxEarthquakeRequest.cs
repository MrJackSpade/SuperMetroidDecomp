namespace SuperMetroid.Core.Fx;

/// <summary>One room-effect request for the global room-shake type and timer bits.</summary>
/// <param name="Type">The room-shake type to install, 0 through 65535.</param>
/// <param name="TimerBits">Bits OR'd into the running shake timer, 0 through 65535.</param>
public readonly record struct RoomFxEarthquakeRequest(
    int Type,
    int TimerBits)
{
    /// <summary>The room-shake type to install; a sixteen-bit word, so a value outside it is rejected.</summary>
    public int Type { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Type);

    /// <summary>Bits OR'd into the running shake timer; a sixteen-bit word, so a value outside it is rejected.</summary>
    public int TimerBits { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, TimerBits);
}

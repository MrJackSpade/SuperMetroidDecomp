namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The NMI frame counters (8- and 16-bit), the lag counter and maximum, and the counter including lag.
/// </summary>
internal sealed class NmiClock
{

    /// <summary>Low-byte NMI frame counter at WRAM <c>$05B5</c>.</summary>
    public byte NmiFrameCounter8 { get; internal set; }

    /// <summary>16-bit accepted-NMI frame counter at WRAM <c>$05B6</c>.</summary>
    public ushort NmiFrameCounter { get; internal set; }

    /// <summary>Consecutive NMIs for which the main loop had no pending request.</summary>
    public ushort NmiLagCounter { get; internal set; }

    /// <summary>Largest observed consecutive lag count.</summary>
    public ushort MaximumNmiLag { get; internal set; }

    /// <summary>Counter incremented by every NMI, accepted or lagged.</summary>
    public ushort NmiCounterIncludingLag { get; internal set; }
}

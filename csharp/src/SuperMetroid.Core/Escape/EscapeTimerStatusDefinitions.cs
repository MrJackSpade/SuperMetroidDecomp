namespace SuperMetroid.Core.Escape;

/// <summary>Named values of the escape timer's cleared state.</summary>
internal static class EscapeTimerStatusDefinitions
{

    /// <summary>
    /// The 8.8 origin the timer's cleared position holds, pixel 128 on each axis with a zero fraction; during startup
    /// the X fraction byte is repurposed as a delay counter.
    /// </summary>
    internal static readonly ushort ClearedOrigin = (ushort)SnesEightEight.FromWhole(128);
}

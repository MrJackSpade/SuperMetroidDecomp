namespace SuperMetroid.Core.Escape;

/// <summary>Named values of the escape timer's cleared state.</summary>
internal static class EscapeTimerStatusDefinitions
{

    /// <summary>
    /// The 8.8 origin the timer's cleared position holds, pixel $80 on each axis with a zero fraction; during startup
    /// the X fraction byte is repurposed as a delay counter.
    /// </summary>
    internal const ushort ClearedOrigin = 0x8000;
}

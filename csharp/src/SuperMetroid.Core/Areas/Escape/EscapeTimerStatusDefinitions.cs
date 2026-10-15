namespace SuperMetroid.Core.Areas.Escape;

/// <summary>Named parts of the escape timer's raw status word at WRAM <c>$0943</c>.</summary>
internal static class EscapeTimerStatusDefinitions
{
    /// <summary>Bit 15, which active timers conventionally keep set beside the low-byte dispatch state.</summary>
    internal const ushort ActiveFlag = 0x8000;

    /// <summary>
    /// The 8.8 origin the timer's cleared position holds, pixel $80 on each axis with a zero fraction; during startup
    /// the X fraction byte is repurposed as a delay counter.
    /// </summary>
    internal const ushort ClearedOrigin = 0x8000;
}

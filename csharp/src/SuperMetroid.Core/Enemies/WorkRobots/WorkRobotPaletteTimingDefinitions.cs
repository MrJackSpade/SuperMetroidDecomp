namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// Cadence and wrap metadata for the Work Robot's glow. The colours each record shows are the import's shades (#1512);
/// this catalog owns the timing <c>AnimatePalette</c> consumes.
/// </summary>
internal static class WorkRobotPaletteTimingDefinitions
{
    /// <summary>Number of authored glow records before the terminator.</summary>
    public const int RecordCount = 6;

    /// <summary>The duration of each authored glow record: a three-record cadence of 64 ticks then 16 and 16.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A8.asm: the six records' timers repeat a
    /// three-record cadence, 64 ticks at i%3==0 and 16 otherwise. The terminator after the last record resolves back to
    /// the first and is never a seventh duration.
    /// </remarks>
    internal static readonly int[] Durations = [64, 16, 16, 64, 16, 16];
}

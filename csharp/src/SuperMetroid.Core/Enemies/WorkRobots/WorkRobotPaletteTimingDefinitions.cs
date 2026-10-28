namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// Compiled cadence and wrap metadata for the Work Robot palette cycle. The four colors
/// in each record remain presentation data; this catalog owns the engine timing consumed
/// by <c>AnimatePalette</c> at <c>$A8:CC67</c>.
/// </summary>
internal static class WorkRobotPaletteTimingDefinitions
{

    /// <summary>Number of authored palette records before the terminator.</summary>
    public const ushort RecordCount = 6;

    /// <summary>Four color words plus one timer word per record.</summary>
    public const ushort RecordByteCount = 10;

    /// <summary>The duration of each authored palette record: a three-record cadence of 64 ticks then 16 and 16.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A8.asm:
    /// the six unsigned timer words at $A8:CCC9+10*i repeat a three-record cadence,
    /// 64 ticks at i%3==0 and 16 otherwise. Validate offsets before division/modulo;
    /// only 0,10,20,30,40,50 are durations. $A8:CCFD is the separate color terminator,
    /// resolved by NormalizeByteOffset at offset 60, never a seventh duration.
    /// </remarks>
    internal static readonly ushort[] Durations = [64, 16, 16, 64, 16, 16];
}

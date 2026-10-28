namespace SuperMetroid.Core.Enemies;

/// <summary>Exact NTSC shared enemy quadratic-speed records, including their authored truncation.</summary>
/// <remarks>
/// Independently reviewed for #1165 against both NTSC J/U v1.0 copies and pinned
/// bank_A0.asm. For record i=0..94, t=i*(i+1)/2; positive velocity has whole=t&gt;&gt;8
/// and fraction=((t&amp;255)*265)&amp;65535. Negate the assembled signed 16.16 value for
/// the negative half, then serialize fraction first, little endian. This exactly matches
/// the originals; the historical generator's provenance remains uncertain.
/// Preserve all word windows 0..758 and displacement windows 0..756, including odd
/// bytes crossing fields and records. These are views of one logical velocity table,
/// not separate fitted curves. VerifyCompiledQuadraticEnemySpeeds checks every window
/// against both original copies. No PAL extrapolation or adjacent-code reads are supported.
/// </remarks>
public static class EnemyQuadraticSpeedDefinitions
{

    /// <summary>The pinned NTSC table contains records zero through 94.</summary>
    public const int RecordCount = 95;

    /// <summary>Each record stores positive fraction/whole followed by negative fraction/whole.</summary>
    public const int RecordSize = 8;

    /// <summary>Native fractional multiplier $0109 applied only to the triangle number's low byte.</summary>
    internal const int FractionMultiplier = 0x0109;
}

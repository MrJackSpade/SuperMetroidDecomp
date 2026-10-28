namespace SuperMetroid.Core.Enemies;

/// <summary>Compiled NTSC fixed-point speeds shared by enemy families.</summary>
/// <remarks>
/// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A0.asm:
/// record i=0..64 stores signed 16.16 velocity i*4096 and its two's-complement negative.
/// Each half stores whole then fraction, with little-endian bytes inside each word.
/// This already implemented algorithm is retained. Read accepts every four-byte window
/// at offsets 0..516, including odd offsets and windows crossing sign/record boundaries;
/// partial windows and overreads remain invalid. The original 520 bytes, bank-A2 mirror,
/// and all windows are checked by VerifyCompiledLinearEnemySpeeds. PAL is a different domain.
/// </remarks>
public static class EnemyLinearSpeedDefinitions
{

    /// <summary>NTSC authored records zero through $40, inclusive; PAL uses different values/count.</summary>
    public const int RecordCount = 65;

    /// <summary>Each record stores positive whole/fraction followed by negative whole/fraction.</summary>
    public const int RecordSize = 8;

    /// <summary>Exact NTSC step $0000.1000, one sixteenth of a pixel per frame.</summary>
    internal const int FixedPointStep = 0x1000;
}

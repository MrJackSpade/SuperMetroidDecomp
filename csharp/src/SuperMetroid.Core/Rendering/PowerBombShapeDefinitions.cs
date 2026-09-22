namespace SuperMetroid.Core.Rendering;

/// <summary>Native unscaled Power Bomb profile used by the continuously expanding phases.</summary>
public static class PowerBombShapeDefinitions
{
    /// <summary>$88:A266, PowerBombExplosion_ShapeDefinitionTable_Unscaled_width: 32 bottom-to-center widths.</summary>
    /// <remarks>
    /// Issue #625 algorithm finding: Width(i) = floor(256*sin(i*pi/64)) for
    /// i in 0..31, exactly EightBitHalfWave[2*i]. All 32 values are independently
    /// verified against the NTSC J/U v1.0 ROM and pinned bank_88.asm by
    /// csharp/tools/LookupTableResearch, including a deterministic decimal-series
    /// candidate with an error interval that cannot cross an integer boundary.
    /// Reject i outside 0..31; the table has no quarter-turn endpoint at i=32.
    /// Keep the renderer's later radius multiplication and right shift separate.
    /// The runtime table remains in place pending consumer/performance validation.
    /// </remarks>
    public static ReadOnlySpan<byte> Widths =>
    [
        0x00,0x0c,0x19,0x25,0x31,0x3e,0x4a,0x56,0x61,0x6d,0x78,0x83,0x8e,0x98,0xa2,0xab,
        0xb5,0xbd,0xc5,0xcd,0xd4,0xdb,0xe1,0xe7,0xec,0xf1,0xf4,0xf8,0xfb,0xfd,0xfe,0xff,
    ];

    /// <summary>$88:A286, PowerBombExplosion_ShapeDefinitionTable_Unscaled_topOffset: inclusive vertical band boundaries.</summary>
    /// <remarks>
    /// Issue #625 algorithm finding: Top(i) = floor(3*floor(256*cos((2*i+1)*pi/128))/4),
    /// i in 0..31. Equivalently, integer arithmetic on EightBitHalfWave[63-2*i]
    /// gives sample*3/4. The half-step phase samples the band boundary; the inner
    /// truncation happens BEFORE the 3/4 vertical scaling. Directly truncating
    /// 192*cos((2*i+1)*pi/128) is wrong at nine of the 32 indices.
    /// LookupTableResearch verifies all entries against this span, the NTSC J/U
    /// v1.0 ROM and pinned bank_88.asm, using the bounded deterministic sine
    /// candidate. Reject indices outside 0..31 and retain the renderer's subsequent
    /// radius scaling and inclusive-band handling. No runtime replacement yet.
    /// </remarks>
    public static ReadOnlySpan<byte> TopOffsets =>
    [
        0xbf,0xbf,0xbe,0xbd,0xba,0xb8,0xb6,0xb2,0xaf,0xab,0xa6,0xa2,0x9c,0x96,0x90,0x8a,
        0x84,0x7d,0x75,0x6e,0x66,0x5e,0x56,0x4d,0x45,0x3c,0x33,0x2a,0x20,0x17,0x0d,0x04,
    ];
}

using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rendering;

/// <summary>Native unscaled Power Bomb profile used by the continuously expanding phases.</summary>
public static class PowerBombShapeDefinitions
{
    /// <summary>
    /// The 192-row quantized quarter-ellipse basis underlying every authored
    /// pre-scaled Power Bomb shape in bank $88. Each of the 4,032 authored bytes
    /// is reproduced by <see cref="ReadPreScaledHalfWidth"/> and was checked
    /// against the supported cartridge during definition extraction.
    /// </summary>
    private static ReadOnlySpan<byte> PreScaledBasis =>
    [
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd, 0xfc, 0xfc, 0xfc,
        0xfc, 0xfb, 0xfb, 0xfb, 0xfb, 0xfb, 0xfa, 0xfa, 0xfa, 0xfa, 0xf9, 0xf9, 0xf9, 0xf8, 0xf8, 0xf7,
        0xf7, 0xf7, 0xf7, 0xf6, 0xf6, 0xf5, 0xf5, 0xf4, 0xf4, 0xf4, 0xf4, 0xf3, 0xf3, 0xf2, 0xf2, 0xf1,
        0xf1, 0xf0, 0xef, 0xef, 0xef, 0xee, 0xee, 0xed, 0xed, 0xec, 0xeb, 0xeb, 0xea, 0xea, 0xe9, 0xe8,
        0xe8, 0xe8, 0xe7, 0xe6, 0xe6, 0xe5, 0xe4, 0xe3, 0xe3, 0xe2, 0xe1, 0xe1, 0xe0, 0xdf, 0xde, 0xdd,
        0xdd, 0xdc, 0xdb, 0xda, 0xd9, 0xd9, 0xd8, 0xd7, 0xd6, 0xd5, 0xd4, 0xd3, 0xd3, 0xd2, 0xd1, 0xd0,
        0xcf, 0xce, 0xcd, 0xcc, 0xcb, 0xca, 0xc9, 0xc7, 0xc6, 0xc5, 0xc4, 0xc3, 0xc2, 0xc1, 0xc0, 0xbf,
        0xbd, 0xbc, 0xbb, 0xba, 0xb9, 0xb8, 0xb6, 0xb5, 0xb3, 0xb2, 0xb1, 0xaf, 0xae, 0xad, 0xab, 0xa9,
        0xa8, 0xa7, 0xa4, 0xa3, 0xa2, 0x9f, 0x9e, 0x9d, 0x9b, 0x99, 0x97, 0x95, 0x93, 0x92, 0x8f, 0x8e,
        0x8b, 0x8a, 0x87, 0x86, 0x83, 0x80, 0x7f, 0x7c, 0x7a, 0x77, 0x74, 0x73, 0x70, 0x6d, 0x6a, 0x67,
        0x63, 0x60, 0x5d, 0x59, 0x56, 0x51, 0x4e, 0x4a, 0x45, 0x3f, 0x3b, 0x35, 0x2d, 0x25, 0x1a, 0x00,
    ];

    /// <summary>Reads one exact $88:9246-$A205 pre-scaled shape byte.</summary>
    public static byte ReadPreScaledHalfWidth(ushort shapePointer, int row)
    {
        if ((uint)row >= SamusSpecialSequenceRomData.PowerBomb.ShapeStride)
            throw new ArgumentOutOfRangeException(nameof(row));

        int frame;
        if (shapePointer >= SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape &&
            shapePointer < SamusSpecialSequenceRomData.PowerBomb.WhiteShapeEnd &&
            (shapePointer - SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape) %
                SamusSpecialSequenceRomData.PowerBomb.ShapeStride == 0)
        {
            frame = (shapePointer - SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape) /
                SamusSpecialSequenceRomData.PowerBomb.ShapeStride;
        }
        else if (shapePointer >= SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape &&
            shapePointer < SamusSpecialSequenceRomData.PowerBomb.YellowShapeEnd &&
            (shapePointer - SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape) %
                SamusSpecialSequenceRomData.PowerBomb.ShapeStride == 0)
        {
            frame = 17 + (shapePointer - SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape) /
                SamusSpecialSequenceRomData.PowerBomb.ShapeStride;
        }
        else
        {
            throw new InvalidDataException(
                $"Power Bomb shape pointer $88:{shapePointer:X4} is outside the compiled profiles.");
        }

        int n = frame < 17 ? frame + 37 : frame - 14;
        int radius = frame < 17
            ? Math.Min(255, 4 + 3 * n * (n - 1) / 32)
            : Math.Min(255, (16 + 192 * n - n * (n - 1)) / 4);
        int sourceRow = Math.Max(0, (256 * row - 1) / radius);
        return sourceRow >= PreScaledBasis.Length
            ? (byte)0
            : (byte)(PreScaledBasis[sourceRow] * radius / 256);
    }
    /// <summary>$88:A266, PowerBombExplosion_ShapeDefinitionTable_Unscaled_width: 32 bottom-to-center widths.</summary>
    /// <remarks>
    /// Issue #625 algorithm finding: Width(i) = floor(256*sin(i*pi/64)) for
    /// i in 0..31, exactly EightBitHalfWave(2*i). All 32 values are independently
    /// verified against the NTSC J/U v1.0 ROM and pinned bank_88.asm by
    /// csharp/tools/LookupTableResearch, including a deterministic decimal-series
    /// candidate with an error interval that cannot cross an integer boundary.
    /// Reject i outside 0..31; the table has no quarter-turn endpoint at i=32.
    /// Keep the renderer's later radius multiplication and right shift separate.
    /// Independently reviewed and retained for #1165: the renderer reads these
    /// 32 bytes repeatedly in each scanline band loop. Keep the compact exact samples
    /// rather than repeat deterministic sine-series evaluation in that hot path.
    /// A generated cache still stores the same table and adds startup machinery.
    /// Individual width-table investigation: #625 / #911.
    /// </remarks>
    public static ReadOnlySpan<byte> Widths =>
    [
        0x00,0x0c,0x19,0x25,0x31,0x3e,0x4a,0x56,0x61,0x6d,0x78,0x83,0x8e,0x98,0xa2,0xab,
        0xb5,0xbd,0xc5,0xcd,0xd4,0xdb,0xe1,0xe7,0xec,0xf1,0xf4,0xf8,0xfb,0xfd,0xfe,0xff,
    ];

    /// <summary>$88:A286, PowerBombExplosion_ShapeDefinitionTable_Unscaled_topOffset: inclusive vertical band boundaries.</summary>
    /// <remarks>
    /// Issue #625 algorithm finding: Top(i) = floor(3*floor(256*cos((2*i+1)*pi/128))/4),
    /// i in 0..31. Equivalently, integer arithmetic on EightBitHalfWave(63-2*i)
    /// gives sample*3/4. The half-step phase samples the band boundary; the inner
    /// truncation happens BEFORE the 3/4 vertical scaling. Directly truncating
    /// 192*cos((2*i+1)*pi/128) is wrong at nine of the 32 indices.
    /// LookupTableResearch verifies all entries against this span, the NTSC J/U
    /// v1.0 ROM and pinned bank_88.asm, using the bounded deterministic sine
    /// candidate. Reject indices outside 0..31 and retain the renderer's subsequent
    /// radius scaling and inclusive-band handling. Independently reviewed and retained
    /// for #1165: these 32 bytes are repeatedly sampled for each rendered scanline.
    /// Recomputing the sine and two quantization stages per band is more expensive
    /// and less direct than the compact exact boundaries; caching recreates the table.
    /// Individual top-offset investigation: #625 / #912.
    /// </remarks>
    public static ReadOnlySpan<byte> TopOffsets =>
    [
        0xbf,0xbf,0xbe,0xbd,0xba,0xb8,0xb6,0xb2,0xaf,0xab,0xa6,0xa2,0x9c,0x96,0x90,0x8a,
        0x84,0x7d,0x75,0x6e,0x66,0x5e,0x56,0x4d,0x45,0x3c,0x33,0x2a,0x20,0x17,0x0d,0x04,
    ];
}

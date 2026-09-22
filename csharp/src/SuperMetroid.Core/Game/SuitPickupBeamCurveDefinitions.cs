namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled upper-half contour for the Varia/Gravity pickup light-beam window.
/// <c>$88:E13E</c> mirrors these 128 offsets across 256 output scanlines.
/// </summary>
internal static class SuitPickupBeamCurveDefinitions
{
    /// <summary>Native source address of the 128-byte contour at <c>$88:E3C9</c>.</summary>
    public const int NativeCurveAddress = 0x88e3c9;

    /// <summary>Number of authored offsets before the native vertical mirror.</summary>
    public const int OffsetCount = 128;

    /// <summary>$88:E3C9, SuitPickup_LightBeam_CurveWidths: 128 upper-half window widths.</summary>
    /// <remarks>
    /// Issue #625 exact bounded model: reject i outside 0..127; for i&lt;7 return
    /// i+1, for i=31 return 16, and otherwise round(24*sqrt(1-((128-i)/127)^2))
    /// to the nearest integer. This reproduces ALL 128 bytes. The opening seven
    /// rows are a unit-width-per-row ramp. Row 31 is an explicit one-pixel authored
    /// correction: the unmodified ellipse returns 15. Its purpose is not established.
    /// Investigation checked ordinary single/double arithmetic, decimal and binary
    /// root quantization through eight fractional places, and two-region integer
    /// midpoint ellipse rasterization. None removes that remaining discrepancy;
    /// midpoint rasterization with the opening ramp likewise differs only at 31.
    /// The broader radius/offset/rounding search did not yield a uniform exact ellipse.
    /// Do not describe this as a proven original generator or silently omit the
    /// correction. csharp/tools/LookupTableResearch verifies the full model against
    /// OffsetAt, the NTSC J/U v1.0 ROM and pinned bank_88.asm, and retains precision
    /// and raster counterexamples. Its final candidate needs no floating point:
    /// for y=128-i, increment w from zero while 127^2*(2*w+1)^2 is at most
    /// 4*24^2*(127^2-y^2). This implements nearest-integer sqrt exactly; ties cannot
    /// occur because the left side is odd and the right divisible by four.
    /// Runtime replacement, lower-half mirroring and performance checks are deferred.
    /// </remarks>
    private static ReadOnlySpan<byte> Offsets =>
    [
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x07,
        0x08, 0x08, 0x09, 0x09, 0x0a, 0x0a, 0x0b, 0x0b,
        0x0b, 0x0c, 0x0c, 0x0c, 0x0d, 0x0d, 0x0d, 0x0e,
        0x0e, 0x0e, 0x0e, 0x0f, 0x0f, 0x0f, 0x0f, 0x10,
        0x10, 0x10, 0x10, 0x10, 0x11, 0x11, 0x11, 0x11,
        0x11, 0x11, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12,
        0x13, 0x13, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14,
        0x14, 0x14, 0x14, 0x14, 0x14, 0x14, 0x15, 0x15,
        0x15, 0x15, 0x15, 0x15, 0x15, 0x15, 0x15, 0x15,
        0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16,
        0x16, 0x16, 0x16, 0x16, 0x17, 0x17, 0x17, 0x17,
        0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17,
        0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x18,
        0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18,
        0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18,
        0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18,
    ];

    /// <summary>Returns one authored upper-half offset.</summary>
    public static byte OffsetAt(int index)
    {
        if ((uint)index >= OffsetCount)
        {
            throw new InvalidDataException(
                $"Suit-pickup beam-curve index {index} is outside the " +
                $"{OffsetCount}-byte native contour.");
        }

        return Offsets[index];
    }
}

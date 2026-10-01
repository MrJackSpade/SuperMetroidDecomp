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
    /// #1165 independently reviewed: retain this authored 128-byte contour after
    /// comparing all original NTSC J/U v1.0 bytes and pinned bank_88.asm, including
    /// the caller's forward/reverse scanline walk. Reject indices outside 0..127.
    /// The ordinary rounded 24-by-127 ellipse needs a seven-row opening ramp and
    /// a row-31 correction (15 versus native 16). Historical research also found a
    /// compatible angle-grid plus Q6 double-rounding model, but neither establishes
    /// the original generator. A deterministic angle/root evaluator and multiple
    /// quantization conventions are less clear than this small direct contour.
    /// Do not introduce an arbitrary row patch or a runtime transcendental evaluator
    /// solely to eliminate storage. The consumer retains 256-line mirroring and its
    /// asymmetric left signed-clamp/right carry-saturation behavior.
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

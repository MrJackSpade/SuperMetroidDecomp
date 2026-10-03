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

    /// <summary>$88:E3C9 SuitPickup_LightBeam_CurveWidths: index 0..127.
    /// The first seven rows open linearly. Remaining rows sample a 24-by-127 ellipse:
    /// round acos((128-index)/127) to an 8,192-step full circle, then round the
    /// horizontal width to six fractional bits before rounding to a whole pixel.</summary>
    /// <remarks>This exact numerical convention independently matches all 128 NTSC
    /// bytes and pinned bank_88.asm, including row 31 where a smooth ellipse differs.
    /// It does not establish which historical tool generated the contour. Decimal
    /// series and bounded angular search preserve deterministic integer results.</remarks>
    public static byte OffsetAt(int index)
    {
        if ((uint)index >= OffsetCount)
        {
            throw new InvalidDataException(
                $"Suit-pickup beam-curve index {index} is outside the " +
                $"{OffsetCount}-byte native contour.");
        }

        if (index < 7) return (byte)(index + 1);
        const decimal pi = 3.1415926535897932384626433833m;
        const decimal step = pi / 4096;
        int y = 128 - index, lower = 0, upper = 2048;
        // Cosine decreases: half-step boundaries select the nearest angular sample.
        while (lower < upper)
        {
            int middle = (lower + upper) / 2;
            decimal boundary = Sine(pi / 2 - (middle + .5m) * step);
            if (y <= 127 * boundary) lower = middle + 1;
            else upper = middle;
        }
        int fixedWidth = (int)(24 * 64 * Sine(lower * step) + .5m);
        return (byte)((fixedWidth + 32) / 64);
    }
    // Arguments stay in [0,pi/2]. Thirteen alternating Taylor terms bound the
    // omitted term below 2e-23; decimal rounding is far below pixel boundaries.
    private static decimal Sine(decimal angle)
    {
        decimal squared = angle * angle;
        decimal term = angle, sum = angle;
        for (int k = 1; k < 13; k++)
        {
            term = -term * squared / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return sum;
    }
}

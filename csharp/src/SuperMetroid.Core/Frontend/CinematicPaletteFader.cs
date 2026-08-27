using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Bank-$8B's cinematic palette cross-fader, represented with the cartridge's 8.8
/// component accumulators rather than interpolated host colors.
/// </summary>
/// <remarks>
/// The native implementation keeps three current-component tables and three step tables
/// in the large <c>tilemap_stuff</c> WRAM union. A target five-bit component is stored as
/// <c>component &lt;&lt; 8</c>; its per-update step is <c>component &lt;&lt; 3</c>, so exactly
/// 32 calls move between black and the target. Keeping those words here reproduces the
/// observable rounding and 16-bit wrapping instead of substituting floating-point lerp.
/// </remarks>
public sealed class CinematicPaletteFader
{
    private readonly ushort[] currentRed = new ushort[SnesCgram.ColorCount];
    private readonly ushort[] currentGreen = new ushort[SnesCgram.ColorCount];
    private readonly ushort[] currentBlue = new ushort[SnesCgram.ColorCount];
    private readonly ushort[] redStep = new ushort[SnesCgram.ColorCount];
    private readonly ushort[] greenStep = new ushort[SnesCgram.ColorCount];
    private readonly ushort[] blueStep = new ushort[SnesCgram.ColorCount];

    /// <summary>Ports <c>DecomposePaletteDataForFading</c> at $8B:8C09.</summary>
    public CinematicPaletteFader(ReadOnlySpan<ushort> targetPalette)
    {
        if (targetPalette.Length != SnesCgram.ColorCount)
        {
            throw new ArgumentException(
                "A cinematic fade target must contain all 256 CGRAM colors.",
                nameof(targetPalette));
        }

        for (int color = 0; color < targetPalette.Length; color++)
        {
            ushort bgr555 = targetPalette[color];
            ushort red = (ushort)(bgr555 & 0x001f);
            ushort green = (ushort)((bgr555 >> 5) & 0x001f);
            ushort blue = (ushort)((bgr555 >> 10) & 0x001f);

            // The first triplet is the live 8.8 value. The second is 1/32 of the target,
            // also in 8.8 form. These shifts are literals from $8B:8C19-$8C59.
            currentRed[color] = (ushort)(red << 8);
            currentGreen[color] = (ushort)(green << 8);
            currentBlue[color] = (ushort)(blue << 8);
            redStep[color] = (ushort)(red << 3);
            greenStep[color] = (ushort)(green << 3);
            blueStep[color] = (ushort)(blue << 3);
        }
    }

    /// <summary>Ports <c>ClearYColorsFromIndexX</c> at $8B:8C5E.</summary>
    /// <param name="paletteByteOffset">
    /// Native X is a byte offset into a BGR555 palette, so $28 means color index $14.
    /// </param>
    /// <param name="colorCount">Native Y, measured in complete colors.</param>
    public void Clear(ushort paletteByteOffset, ushort colorCount)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(paletteByteOffset, colorCount);
        for (int color = firstColor; color < exclusiveEnd; color++)
        {
            currentRed[color] = 0;
            currentGreen[color] = 0;
            currentBlue[color] = 0;
        }
    }

    /// <summary>Ports <c>FadeOutYColorsFromIndexX</c> at $8B:8C83.</summary>
    public void FadeOut(ushort paletteByteOffset, ushort colorCount)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(paletteByteOffset, colorCount);
        for (int color = firstColor; color < exclusiveEnd; color++)
        {
            // Native SBC stores a wrapping 16-bit result. The known intro ranges reach
            // exactly zero after 32 calls, but preserving wrap makes this helper faithful
            // and makes accidental over-stepping visible in a debugger.
            currentRed[color] = unchecked((ushort)(currentRed[color] - redStep[color]));
            currentGreen[color] = unchecked((ushort)(currentGreen[color] - greenStep[color]));
            currentBlue[color] = unchecked((ushort)(currentBlue[color] - blueStep[color]));
        }
    }

    /// <summary>Ports <c>FadeInYColorsFromIndexX</c> at $8B:8CB2.</summary>
    public void FadeIn(ushort paletteByteOffset, ushort colorCount)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(paletteByteOffset, colorCount);
        for (int color = firstColor; color < exclusiveEnd; color++)
        {
            // The $1FFF mask is not a generic clamp. It is present in each of the three
            // retail ADC paths and must occur after every addition.
            currentRed[color] = (ushort)((currentRed[color] + redStep[color]) & 0x1fff);
            currentGreen[color] = (ushort)((currentGreen[color] + greenStep[color]) & 0x1fff);
            currentBlue[color] = (ushort)((currentBlue[color] + blueStep[color]) & 0x1fff);
        }
    }

    /// <summary>Ports <c>ComposeFadingPalettes</c> at $8B:8CEA into live CGRAM.</summary>
    public void ComposeInto(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < SnesCgram.ColorCount; color++)
        {
            // High bytes supply the visible five-bit components. Writing through
            // SnesCgram matches the following native NMI upload into physical CGRAM.
            ushort bgr555 = (ushort)(
                ((currentRed[color] >> 8) & 0x001f) |
                ((currentGreen[color] >> 3) & 0x03e0) |
                ((currentBlue[color] << 2) & 0x7c00));
            cgram.SetColor(color, bgr555);
        }
    }

    private static (int FirstColor, int ExclusiveEnd) ValidateRange(
        ushort paletteByteOffset,
        ushort colorCount)
    {
        if ((paletteByteOffset & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(paletteByteOffset), "Palette byte offsets must be word-aligned.");
        if (colorCount == 0)
            throw new ArgumentOutOfRangeException(nameof(colorCount), "The native do/while routine requires at least one color.");

        int firstColor = paletteByteOffset / 2;
        int exclusiveEnd = firstColor + colorCount;
        if (exclusiveEnd > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(colorCount), "Palette range leaves the 256-color buffer.");
        return (firstColor, exclusiveEnd);
    }
}

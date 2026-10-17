namespace SuperMetroid.Core.Frontend.Intro;

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
    /// <summary>Current red 8.8 component for each of 256 CGRAM colors.</summary>
    private readonly ushort[] currentRed = new ushort[CgramPalettes.ColorCount];
    /// <summary>Current green 8.8 component for each CGRAM color.</summary>
    private readonly ushort[] currentGreen = new ushort[CgramPalettes.ColorCount];
    /// <summary>Current blue 8.8 component for each CGRAM color.</summary>
    private readonly ushort[] currentBlue = new ushort[CgramPalettes.ColorCount];
    /// <summary>One thirty-second of each red target component in 8.8 form.</summary>
    private readonly ushort[] redStep = new ushort[CgramPalettes.ColorCount];
    /// <summary>One thirty-second of each green target component in 8.8 form.</summary>
    private readonly ushort[] greenStep = new ushort[CgramPalettes.ColorCount];
    /// <summary>One thirty-second of each blue target component in 8.8 form.</summary>
    private readonly ushort[] blueStep = new ushort[CgramPalettes.ColorCount];

    /// <summary>Ports <c>DecomposePaletteDataForFading</c> at $8B:8C09.</summary>
    public CinematicPaletteFader(ReadOnlySpan<Bgr555> targetPalette)
    {
        Ensure.LengthEqual(targetPalette, CgramPalettes.ColorCount);

        for (int color = 0; color < targetPalette.Length; color++)
        {
            ushort red = (ushort)targetPalette[color].Red;
            ushort green = (ushort)targetPalette[color].Green;
            ushort blue = (ushort)targetPalette[color].Blue;

            // The first triplet is the live 8.8 value. The second is 1/32 of the target,
            // also in 8.8 form. These shifts are literals from $8B:8C19-$8C59.
            currentRed[color] = (ushort)SnesEightEight.FromWhole(red);
            currentGreen[color] = (ushort)SnesEightEight.FromWhole(green);
            currentBlue[color] = (ushort)SnesEightEight.FromWhole(blue);
            redStep[color] = (ushort)(red << 3);
            greenStep[color] = (ushort)(green << 3);
            blueStep[color] = (ushort)(blue << 3);
        }
    }

    /// <summary>Ports <c>ClearYColorsFromIndexX</c> at $8B:8C5E: the span's colors become black.</summary>
    /// <param name="span">The colors to clear.</param>
    public void Clear(IntroPaletteSpan span)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(span);
        for (int color = firstColor; color < exclusiveEnd; color++)
        {
            currentRed[color] = 0;
            currentGreen[color] = 0;
            currentBlue[color] = 0;
        }
    }

    /// <summary>Ports <c>FadeOutYColorsFromIndexX</c> at $8B:8C83: the span's colors take one step toward black.</summary>
    /// <param name="span">The colors to fade.</param>
    public void FadeOut(IntroPaletteSpan span)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(span);
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

    /// <summary>Ports <c>FadeInYColorsFromIndexX</c> at $8B:8CB2: the span's colors take one step toward their target.</summary>
    /// <param name="span">The colors to fade.</param>
    public void FadeIn(IntroPaletteSpan span)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(span);
        for (int color = firstColor; color < exclusiveEnd; color++)
        {
            // The wrap to thirteen bits (the $1FFF mask) is not a generic clamp. It is present
            // in each of the three retail ADC paths and must occur after every addition.
            currentRed[color] = (ushort)((currentRed[color] + redStep[color]) % 8192);
            currentGreen[color] = (ushort)((currentGreen[color] + greenStep[color]) % 8192);
            currentBlue[color] = (ushort)((currentBlue[color] + blueStep[color]) % 8192);
        }
    }

    /// <summary>Ports <c>ComposeFadingPalettes</c> at $8B:8CEA into live CGRAM.</summary>
    public void ComposeInto(SnesCgram cgram)
    {
        Ensure.NotNull(cgram);
        Span<Bgr555> image = stackalloc Bgr555[CgramPalettes.ColorCount];
        for (int color = 0; color < image.Length; color++)
        {
            // High bytes supply the visible five-bit components. Writing through
            // SnesCgram matches the following native NMI upload into physical CGRAM.
            image[color] = new Bgr555(
                (SnesLittleEndian.HighByte(currentRed[color])) & Bgr555.MaxChannel,
                (SnesLittleEndian.HighByte(currentGreen[color])) & Bgr555.MaxChannel,
                (SnesLittleEndian.HighByte(currentBlue[color])) & Bgr555.MaxChannel);
        }
        new CgramPalettes(cgram).LoadImage(image);
    }

    /// <summary>Converts a nonempty span to a bounded palette color interval.</summary>
    private static (int FirstColor, int ExclusiveEnd) ValidateRange(IntroPaletteSpan span)
    {
        // The native do/while routine handles at least one color, and the run stays inside the 256-color buffer.
        int firstColor = Ensure.BetweenInclusive(0, CgramPalettes.ColorCount - 1, span.FirstColor);
        int colorCount = Ensure.BetweenInclusive(1, CgramPalettes.ColorCount - firstColor, span.ColorCount);
        return (firstColor, firstColor + colorCount);
    }
}

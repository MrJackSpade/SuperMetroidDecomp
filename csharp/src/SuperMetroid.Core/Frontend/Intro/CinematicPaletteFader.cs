namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Bank-$8B's cinematic palette cross-fader, represented with the cartridge's 8.8
/// component accumulators rather than interpolated host colors.
/// </summary>
/// <remarks>
/// The native implementation keeps three current-component tables and three step tables
/// in the large <c>tilemap_stuff</c> WRAM union; here each colour's three components are one <see cref="CinematicFadeColor"/>. A target five-bit component is stored as
/// <c>component &lt;&lt; 8</c>; its per-update step is <c>component &lt;&lt; 3</c>, so exactly
/// 32 calls move between black and the target. Keeping those words here reproduces the
/// observable rounding and 16-bit wrapping instead of substituting floating-point lerp.
/// </remarks>
public sealed class CinematicPaletteFader
{
    /// <summary>Current 8.8 colour for each of 256 CGRAM colors.</summary>
    private readonly CinematicFadeColor[] current = new CinematicFadeColor[CgramPalettes.ColorCount];
    /// <summary>One thirty-second of each target colour in 8.8 form.</summary>
    private readonly CinematicFadeColor[] step = new CinematicFadeColor[CgramPalettes.ColorCount];

    /// <summary>Ports <c>DecomposePaletteDataForFading</c> at $8B:8C09.</summary>
    public CinematicPaletteFader(ReadOnlySpan<Bgr555> targetPalette)
    {
        Ensure.LengthEqual(targetPalette, CgramPalettes.ColorCount);

        for (int color = 0; color < targetPalette.Length; color++)
        {
            current[color] = CinematicFadeColor.Target(targetPalette[color]);
            step[color] = CinematicFadeColor.Step(targetPalette[color]);
        }
    }

    /// <summary>Ports <c>ClearYColorsFromIndexX</c> at $8B:8C5E: the span's colors become black.</summary>
    /// <param name="span">The colors to clear.</param>
    public void Clear(IntroPaletteSpan span)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(span);
        Array.Fill(current, default, firstColor, exclusiveEnd - firstColor);
    }

    /// <summary>Ports <c>FadeOutYColorsFromIndexX</c> at $8B:8C83: the span's colors take one step toward black.</summary>
    /// <param name="span">The colors to fade.</param>
    public void FadeOut(IntroPaletteSpan span)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(span);
        // The known intro ranges reach exactly zero after 32 calls, but preserving the native
        // wrap makes this helper faithful and makes accidental over-stepping visible in a debugger.
        for (int color = firstColor; color < exclusiveEnd; color++)
            current[color] = current[color].Minus(step[color]);
    }

    /// <summary>Ports <c>FadeInYColorsFromIndexX</c> at $8B:8CB2: the span's colors take one step toward their target.</summary>
    /// <param name="span">The colors to fade.</param>
    public void FadeIn(IntroPaletteSpan span)
    {
        (int firstColor, int exclusiveEnd) = ValidateRange(span);
        for (int color = firstColor; color < exclusiveEnd; color++)
            current[color] = current[color].PlusWrapped(step[color]);
    }

    /// <summary>Ports <c>ComposeFadingPalettes</c> at $8B:8CEA into live CGRAM.</summary>
    public void ComposeInto(IPaletteColors cgram)
    {
        Ensure.NotNull(cgram);
        Span<Bgr555> image = stackalloc Bgr555[CgramPalettes.ColorCount];
        for (int color = 0; color < image.Length; color++)
            image[color] = current[color].Visible;
        new CgramPalettes(cgram).LoadImage(image);
    }

    /// <summary>Converts a nonempty span to a bounded palette color interval.</summary>
    private static PaletteColorRange ValidateRange(IntroPaletteSpan span)
    {
        // The native do/while routine handles at least one color, and the run stays inside the 256-color buffer.
        int firstColor = Ensure.BetweenInclusive(0, CgramPalettes.ColorCount - 1, span.FirstColor);
        int colorCount = Ensure.BetweenInclusive(1, CgramPalettes.ColorCount - firstColor, span.ColorCount);
        return new PaletteColorRange(firstColor, firstColor + colorCount);
    }
}

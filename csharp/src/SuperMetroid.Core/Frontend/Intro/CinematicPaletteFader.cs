using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
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
/// The fader holds one accumulator per screen colour and addresses them by the screen's colour groups (#1464).
/// </remarks>
public sealed class CinematicPaletteFader
{
    /// <summary>Current 8.8 colour for each of the screen's colours.</summary>
    private readonly CinematicFadeColor[] current = new CinematicFadeColor[ScreenPalette.ColorCount];
    /// <summary>One thirty-second of each target colour in 8.8 form.</summary>
    private readonly CinematicFadeColor[] step = new CinematicFadeColor[ScreenPalette.ColorCount];

    /// <summary>Ports <c>DecomposePaletteDataForFading</c> at $8B:8C09.</summary>
    public CinematicPaletteFader(ReadOnlySpan<RgbColor> targetPalette)
    {
        Ensure.LengthEqual(targetPalette, ScreenPalette.ColorCount);

        for (int color = 0; color < targetPalette.Length; color++)
        {
            current[color] = CinematicFadeColor.Target(targetPalette[color]);
            step[color] = CinematicFadeColor.Step(targetPalette[color]);
        }
    }

    /// <summary>Ports <c>ClearYColorsFromIndexX</c> at $8B:8C5E: the group's colours become black.</summary>
    /// <param name="group">The colours to clear.</param>
    internal void Clear(ScreenColourGroup group) => ScreenColourGroups.InPlace(current.AsSpan(), group).Clear();

    /// <summary>Ports <c>FadeOutYColorsFromIndexX</c> at $8B:8C83: the group's colours take one step toward black.</summary>
    /// <param name="group">The colours to fade.</param>
    internal void FadeOut(ScreenColourGroup group)
    {
        Span<CinematicFadeColor> colours = ScreenColourGroups.InPlace(current.AsSpan(), group);
        ReadOnlySpan<CinematicFadeColor> steps = ScreenColourGroups.In<CinematicFadeColor>(step, group);
        // The known intro groups reach exactly zero after 32 calls, but preserving the native
        // wrap makes this helper faithful and makes accidental over-stepping visible in a debugger.
        for (int color = 0; color < colours.Length; color++)
            colours[color] = colours[color].Minus(steps[color]);
    }

    /// <summary>Ports <c>FadeInYColorsFromIndexX</c> at $8B:8CB2: the group's colours take one step toward their target.</summary>
    /// <param name="group">The colours to fade.</param>
    internal void FadeIn(ScreenColourGroup group)
    {
        Span<CinematicFadeColor> colours = ScreenColourGroups.InPlace(current.AsSpan(), group);
        ReadOnlySpan<CinematicFadeColor> steps = ScreenColourGroups.In<CinematicFadeColor>(step, group);
        for (int color = 0; color < colours.Length; color++)
            colours[color] = colours[color].PlusWrapped(steps[color]);
    }

    /// <summary>Ports <c>ComposeFadingPalettes</c> at $8B:8CEA into the screen's colours.</summary>
    public void ComposeInto(ScreenPalette colours)
    {
        Ensure.NotNull(colours);
        Span<RgbColor> image = stackalloc RgbColor[ScreenPalette.ColorCount];
        for (int color = 0; color < image.Length; color++)
            image[color] = current[color].Visible;
        colours.Load(image);
    }
}

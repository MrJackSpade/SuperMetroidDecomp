using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's colours show (#1512): each colour load, fade level, flash, logo crossfade step and palette-effect record
/// its scenes have shown, in the order they were shown, so that where two recolour the same colours the later shows, and
/// the backdrop the last of them that covers it shows. Every picture and floor the ending draws is shaded in all of them;
/// an actor's palette-inheriting parts in each of them under its colouring. Which colours each covers is the import's.
/// </summary>
internal sealed class EndingColours
{
    /// <summary>The shades shown, least recently shown first; each family keeps only its last.</summary>
    private readonly List<PictureShade> shown = [];

    /// <summary>The solid colour shown behind every layer, where nothing is drawn.</summary>
    internal RgbColor Backdrop { get; private set; } = RgbColor.Black;

    /// <summary>
    /// Shows the installed colours <paramref name="load"/>, and <paramref name="backdrop"/> behind every layer when the load
    /// covers the backdrop. The escape's whole screen replaces everything shown before.
    /// </summary>
    internal void Load(EndingColourLoad load, RgbColor? backdrop)
    {
        if (load == EndingColourLoad.Escape)
            shown.Clear();
        Show(EndingShades.Loaded(load), backdrop);
    }

    /// <summary>Shows <paramref name="colours"/> at fade <paramref name="level"/> of <see cref="EndingShades.FadeLevels"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The level is outside the fade.</exception>
    internal void Fade(EndingFade colours, int level) =>
        Show(EndingShades.Faded(colours, Ensure.BetweenInclusive(0, EndingShades.FadeLevels, level)), null);

    /// <summary>Shows <paramref name="flash"/>, and <paramref name="backdrop"/> behind every layer when the flash covers it.</summary>
    internal void Flash(EndingFlash flash, RgbColor? backdrop) => Show(EndingShades.Flashed(flash), backdrop);

    /// <summary>Shows the logo crossfade's <paramref name="step"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is outside the crossfade.</exception>
    internal void LogoCrossfade(int step) =>
        Show(EndingShades.LogoCrossfade(Ensure.BetweenInclusive(0, EndingShades.LogoCrossfadeSteps - 1, step)), null);

    /// <summary>
    /// Shows the palette-effect <paramref name="record"/> in <paramref name="colours"/>, and <paramref name="backdrop"/>
    /// behind every layer when the record covers it.
    /// </summary>
    internal void PaletteFx(PaletteFxColours colours, PaletteFxFrame record, RgbColor? backdrop) =>
        Show(PaletteFxShades.Of(colours, record), backdrop);

    /// <summary>The shading every ending picture and floor shows: each shade shown, in order.</summary>
    internal PictureShading Shading => new([.. shown], []);

    /// <summary>
    /// The shading an actor's frame shows when its palette-inheriting parts draw in the colouring whose drawn shade is
    /// <paramref name="drawn"/>: that shade, then each shade shown, over the frame's own-palette parts and under the
    /// colouring over its inheriting parts, in order.
    /// </summary>
    internal PictureShading Drawn(PictureShade drawn)
    {
        ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>(1 + 2 * shown.Count);
        shades.Add(drawn);
        foreach (PictureShade screen in shown)
        {
            shades.Add(screen);
            shades.Add(EndingShades.Under(drawn, screen));
        }
        return new PictureShading(shades.MoveToImmutable(), []);
    }

    /// <summary>Shows <paramref name="shade"/> over every shade shown before, replacing its family's earlier one.</summary>
    private void Show(PictureShade shade, RgbColor? backdrop)
    {
        shown.RemoveAll(earlier => earlier.Effect == shade.Effect);
        shown.Add(shade);
        if (backdrop is { } colour)
            Backdrop = colour;
    }
}

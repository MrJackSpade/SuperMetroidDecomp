using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the running ending scene's colours show (#1512, #1544): the scene's own colours, then each colour load, fade level,
/// flash, logo crossfade step and palette-effect record the scene has shown since, in the order shown. A scene's colours
/// are its own: a scene starts in its whole screen of colours, and nothing an earlier scene showed shows in it; a palette
/// effect's records stop showing when the effects are cleared. Each background picture and floor the ending draws is
/// shaded in those authored for it, and an actor's pictures in those authored for it under its colouring (#1544). Which
/// colours each covers is the import's.
/// </summary>
internal sealed class EndingColours
{
    /// <summary>The shades the running scene shows, least recently shown first; each family keeps only its last.</summary>
    internal readonly List<PictureShade> shown = [];

    /// <summary>The colour groups whose palette-effect records <see cref="shown"/> holds.</summary>
    private readonly List<string> paletteFxEffects = [];

    /// <summary>The solid colour shown behind every layer, where nothing is drawn.</summary>
    internal RgbColor Backdrop { get; private set; } = RgbColor.Black;

    /// <summary>
    /// Shows the installed colours <paramref name="load"/>, and <paramref name="backdrop"/> behind every layer when the load
    /// covers the backdrop. A scene's whole screen of colours starts that scene: nothing shown before it shows any more.
    /// </summary>
    internal void Load(EndingColourLoad load, RgbColor? backdrop)
    {
        if (StartsScene(load))
        {
            shown.Clear();
            paletteFxEffects.Clear();
        }
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
    internal void PaletteFx(PaletteFxColours colours, PaletteFxFrame record, RgbColor? backdrop)
    {
        PictureShade shade = PaletteFxShades.Of(colours, record);
        if (!paletteFxEffects.Contains(shade.Effect))
            paletteFxEffects.Add(shade.Effect);
        Show(shade, backdrop);
    }

    /// <summary>
    /// Ends every palette effect's records (#1544): the effects that showed them are cleared, so their colours stop
    /// showing. The backdrop a record showed stays until the scene shows another.
    /// </summary>
    internal void EndPaletteFx()
    {
        shown.RemoveAll(shade => paletteFxEffects.Contains(shade.Effect));
        paletteFxEffects.Clear();
    }

    /// <summary>
    /// The shading <paramref name="scenery"/>, a background picture or floor, shows (#1544): each shade shown that is
    /// authored for it (<see cref="EndingColourOwners"/>), in order. A shade authored for an actor or for other scenery is
    /// not shown on it, even where the two share a colour group.
    /// </summary>
    /// <exception cref="InvalidDataException">A shade shown is of no colour effect the ending shows.</exception>
    internal PictureShading SceneryShading(EndingScenery scenery)
    {
        ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>(shown.Count);
        foreach (PictureShade screen in shown)
        {
            if (EndingColourOwners.Shows(scenery, screen))
                shades.Add(screen);
        }
        return new PictureShading(shades.ToImmutable(), []);
    }

    /// <summary>
    /// The shading an actor's frame shows when it draws in the colouring whose drawn shade is <paramref name="drawn"/>
    /// (#1544): that shade, then each shade shown that is authored for that colouring's actors
    /// (<see cref="EndingColourOwners"/>), under the colouring, in order. A shade authored for another actor is not shown on
    /// the frame, even where the two once shared a colour group.
    /// </summary>
    /// <exception cref="InvalidDataException">The shade is no ending actor colouring's drawn shade, or a shade shown is of no colour effect the ending shows.</exception>
    internal PictureShading Drawn(PictureShade drawn)
    {
        EndingSpriteColouring colouring = EndingColourOwners.ColouringOf(drawn);
        ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>(1 + shown.Count);
        shades.Add(drawn);
        foreach (PictureShade screen in shown)
        {
            if (EndingColourOwners.Shows(colouring, screen))
                shades.Add(EndingShades.Under(drawn, screen));
        }
        return new PictureShading(shades.ToImmutable(), []);
    }

    /// <summary>Whether <paramref name="load"/> is a scene's whole screen of colours, which starts that scene.</summary>
    private static bool StartsScene(EndingColourLoad load) =>
        load is EndingColourLoad.Escape or EndingColourLoad.Explosion or EndingColourLoad.Credits or EndingColourLoad.PostCredits;

    /// <summary>Shows <paramref name="shade"/> over every shade shown before, replacing its family's earlier one.</summary>
    private void Show(PictureShade shade, RgbColor? backdrop)
    {
        shown.RemoveAll(earlier => earlier.Effect == shade.Effect);
        shown.Add(shade);
        if (backdrop is { } colour)
            Backdrop = colour;
    }
}

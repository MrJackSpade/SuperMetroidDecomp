using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's cinematic cross-fader (#1512): each faded colour group sits at a level between black and its installed
/// colours, and every step moves a group one level toward black or toward its colours. The pictures that show a group
/// name the shade of its level, which the import pre-colours; the narration text, which lies in one group, shows its
/// level as a colour effect.
/// </summary>
/// <remarks>
/// <see cref="Levels"/> steps separate black from a colour, and a colour at level <c>n</c> shows each channel scaled by
/// <c>n / Levels</c>, rounded down. That is the rounding the original's per-channel fixed-point accumulators produce, so
/// an early fade-in step can still show black. A fader is made for each transition, which starts every colour at full.
/// Two groups may hold some of the same colours, but no transition moves both, so a group below full is the one moving
/// those colours.
/// </remarks>
public sealed class CinematicPaletteFader
{
    /// <summary>The steps between black and a colour.</summary>
    internal const int Levels = 32;

    /// <summary>Each background group's level, from zero (black) to <see cref="Levels"/> (its colours).</summary>
    private readonly Dictionary<IntroBackgroundFade, int> backgrounds =
        Enum.GetValues<IntroBackgroundFade>().ToDictionary(static colours => colours, static _ => Levels);
    /// <summary>Each sprite group's level, from zero (black) to <see cref="Levels"/> (its colours).</summary>
    private readonly Dictionary<IntroSpriteFade, int> sprites =
        Enum.GetValues<IntroSpriteFade>().ToDictionary(static colours => colours, static _ => Levels);

    /// <summary>Ports <c>ClearYColorsFromIndexX</c>: the groups' colours become black.</summary>
    /// <param name="colours">The colours to clear.</param>
    internal void Clear(IntroFadeColours colours)
    {
        Ensure.NotNull(colours);
        foreach (IntroBackgroundFade group in colours.Backgrounds)
            backgrounds[group] = 0;
        foreach (IntroSpriteFade group in colours.Sprites)
            sprites[group] = 0;
    }

    /// <summary>Ports <c>FadeOutYColorsFromIndexX</c>: the groups' colours take one step toward black.</summary>
    /// <param name="colours">The colours to fade.</param>
    /// <exception cref="InvalidOperationException">A group was faded past black.</exception>
    internal void FadeOut(IntroFadeColours colours) => Step(colours, -1);

    /// <summary>Ports <c>FadeInYColorsFromIndexX</c>: the groups' colours take one step toward their installed colours.</summary>
    /// <param name="colours">The colours to fade.</param>
    /// <exception cref="InvalidOperationException">A group was faded past its installed colours.</exception>
    internal void FadeIn(IntroFadeColours colours) => Step(colours, 1);

    /// <summary>The level <paramref name="colours"/> shows.</summary>
    internal int LevelOf(IntroBackgroundFade colours) => backgrounds[colours];

    /// <summary>The level <paramref name="colours"/> shows.</summary>
    internal int LevelOf(IntroSpriteFade colours) => sprites[colours];

    /// <summary>
    /// The level the narration's colours show: that of the narration group below full, as no transition moves both
    /// groups that hold them.
    /// </summary>
    internal int NarrationLevel => IntroCinematicDefinitions.Palette.NarrationColours.Min(colours => LevelOf(colours));

    /// <summary>
    /// The narration text's shading (#1512): the narration's level as a colour effect over its glow colours, or its glow
    /// colours alone at full.
    /// </summary>
    internal PictureShading NarrationShading(PictureShading glow) =>
        NarrationLevel == Levels ? Ensure.NotNull(glow) : Ensure.NotNull(glow).With(new ColourLevel(NarrationLevel, Levels));

    /// <summary>A scene picture's shading: the shade of each background group's level below full.</summary>
    internal PictureShading SceneShading
    {
        get
        {
            ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>();
            foreach ((IntroBackgroundFade group, int level) in backgrounds)
            {
                if (level < Levels)
                    shades.Add(IntroShades.Faded(group, level));
            }
            return new PictureShading(shades.ToImmutable(), []);
        }
    }

    /// <summary>
    /// An actor's shading: <paramref name="drawn"/>, then each sprite group's level below full over the actor's own-palette
    /// parts and over the parts its drawn shade colours.
    /// </summary>
    internal PictureShading ActorShading(PictureShade drawn)
    {
        ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>();
        shades.Add(drawn);
        foreach ((IntroSpriteFade group, int level) in sprites)
        {
            if (level >= Levels)
                continue;
            shades.Add(IntroShades.Faded(group, level));
            shades.Add(IntroShades.Faded(group, level, drawn));
        }
        return new PictureShading(shades.ToImmutable(), []);
    }

    /// <summary>Moves every group of <paramref name="colours"/> by <paramref name="step"/> levels.</summary>
    /// <exception cref="InvalidOperationException">A group was faded past black or past its installed colours.</exception>
    private void Step(IntroFadeColours colours, int step)
    {
        Ensure.NotNull(colours);
        foreach (IntroBackgroundFade group in colours.Backgrounds)
            backgrounds[group] = Moved(backgrounds[group], step, group.ToString());
        foreach (IntroSpriteFade group in colours.Sprites)
            sprites[group] = Moved(sprites[group], step, group.ToString());
    }

    /// <summary><paramref name="level"/> moved by <paramref name="step"/>.</summary>
    /// <exception cref="InvalidOperationException">The level leaves black through full.</exception>
    private static int Moved(int level, int step, string group)
    {
        int moved = level + step;
        if ((uint)moved > Levels)
            throw new InvalidOperationException($"The opening's {group} colours were faded to level {moved}, outside 0 through {Levels}.");
        return moved;
    }
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The shades the import pre-colours the opening's sheets in (#1512, #1597): each colouring an actor draws in, each
/// crossfade level below full of each crossfaded scene over the scene pictures and actors, and each colour variant a
/// narration letter's typing or glow gives it. Which inks each recolours, and to what, is the import's.
/// </summary>
public static class IntroShades
{
    /// <summary>The shade family of the narration letters' colour variants.</summary>
    public const string GlowEffect = "IntroTextGlow";

    /// <summary>An actor's palette-inheriting parts drawn in <paramref name="colouring"/>.</summary>
    public static PictureShade Drawn(IntroSpriteColouring colouring) => new($"Intro{colouring}", 0);

    /// <summary>
    /// <paramref name="scene"/>'s colours at crossfade <paramref name="level"/>, below full, on the scene pictures or on the
    /// intro sprite sheet's own-palette parts.
    /// </summary>
    public static PictureShade Faded(IntroCrossfadeScene scene, int level) => new($"IntroFade{scene}", level);

    /// <summary>
    /// <paramref name="scene"/>'s colours an actor drawn in <paramref name="drawn"/> shows at crossfade
    /// <paramref name="level"/>, below full, over the inks its drawn shade recolours.
    /// </summary>
    public static PictureShade Faded(IntroCrossfadeScene scene, int level, PictureShade drawn) =>
        new($"{drawn.Effect}.IntroFade{scene}", level);

    /// <summary>A narration letter in the colour <paramref name="variant"/> its typing or glow gives it.</summary>
    public static PictureShade Glow(int variant) => new(GlowEffect, variant);
}

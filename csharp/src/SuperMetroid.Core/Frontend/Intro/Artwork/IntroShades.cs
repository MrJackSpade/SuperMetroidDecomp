using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The shades the import pre-colours the opening's sheets in (#1512): each colouring an actor draws in, each cross-fade
/// level below full of each faded colour group over the scene pictures and actors, and each colour variant a narration
/// letter's typing or glow gives it. Which inks each recolours, and to what, is the import's.
/// </summary>
public static class IntroShades
{
    /// <summary>The shade family of the narration letters' colour variants.</summary>
    public const string GlowEffect = "IntroTextGlow";

    /// <summary>An actor's palette-inheriting parts drawn in <paramref name="colouring"/>.</summary>
    public static PictureShade Drawn(IntroSpriteColouring colouring) => new($"Intro{colouring}", 0);

    /// <summary>The scene pictures' <paramref name="colours"/> at cross-fade <paramref name="level"/>, below full.</summary>
    public static PictureShade Faded(IntroBackgroundFade colours, int level) => new($"IntroFade{colours}", level);

    /// <summary>The intro sprite sheet's own-palette <paramref name="colours"/> at cross-fade <paramref name="level"/>, below full.</summary>
    public static PictureShade Faded(IntroSpriteFade colours, int level) => new($"IntroFade{colours}", level);

    /// <summary>
    /// The <paramref name="colours"/> an actor drawn in <paramref name="drawn"/> shows at cross-fade <paramref name="level"/>,
    /// below full, over the inks its drawn shade recolours.
    /// </summary>
    public static PictureShade Faded(IntroSpriteFade colours, int level, PictureShade drawn) =>
        new($"{drawn.Effect}.IntroFade{colours}", level);

    /// <summary>A narration letter in the colour <paramref name="variant"/> its typing or glow gives it.</summary>
    public static PictureShade Glow(int variant) => new(GlowEffect, variant);
}

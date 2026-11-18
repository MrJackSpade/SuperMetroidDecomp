using System.Collections.Frozen;
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

    /// <summary>
    /// The installed name of each actor colouring's drawn shade (#2511), written out so renaming a colouring's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<IntroSpriteColouring, string> DrawnKeys =
        new Dictionary<IntroSpriteColouring, string>
        {
            [IntroSpriteColouring.MotherBrain] = "IntroMotherBrain",
            [IntroSpriteColouring.CrossFadeActors] = "IntroCrossFadeActors",
            [IntroSpriteColouring.DiscoveryActors] = "IntroDiscoveryActors",
            [IntroSpriteColouring.Explosions] = "IntroExplosions",
        }.ToFrozenDictionary();

    /// <summary>
    /// The installed name of each crossfaded scene's fade shades (#2511), written out so renaming a scene's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<IntroCrossfadeScene, string> FadeKeys =
        new Dictionary<IntroCrossfadeScene, string>
        {
            [IntroCrossfadeScene.Narration] = "IntroFadeNarration",
            [IntroCrossfadeScene.Flashback] = "IntroFadeFlashback",
            [IntroCrossfadeScene.Laboratory] = "IntroFadeLaboratory",
        }.ToFrozenDictionary();

    /// <summary>An actor's recolourable parts drawn in <paramref name="colouring"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring has no installed name.</exception>
    public static PictureShade Drawn(IntroSpriteColouring colouring) => new(Ensure.Listed(DrawnKeys, colouring), 0);

    /// <summary>
    /// <paramref name="scene"/>'s colours at crossfade <paramref name="level"/>, below full, on the scene pictures or on the
    /// intro sprite sheet's own-colour parts.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The scene has no installed name.</exception>
    public static PictureShade Faded(IntroCrossfadeScene scene, int level) => new(Ensure.Listed(FadeKeys, scene), level);

    /// <summary>
    /// <paramref name="scene"/>'s colours an actor drawn in <paramref name="drawn"/> shows at crossfade
    /// <paramref name="level"/>, below full, over the inks its drawn shade recolours.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The scene has no installed name.</exception>
    public static PictureShade Faded(IntroCrossfadeScene scene, int level, PictureShade drawn) =>
        new($"{drawn.Effect}.{Ensure.Listed(FadeKeys, scene)}", level);

    /// <summary>A narration letter in the colours of glow step <paramref name="step"/>, which its typing or glow gives it.</summary>
    public static PictureShade Glow(IntroTextGlowStep step) => new(GlowEffect, step.Step);
}

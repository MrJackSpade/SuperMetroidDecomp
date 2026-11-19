using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The shades the import pre-colours the ending's sheets in (#1512): each colour load, each level of each faded group,
/// each flash and each logo crossfade step, over every ending picture and floor, and each of those authored for an actor
/// (#1544) under the colouring it draws in, over its pictures. Which inks each recolours, and to what, is the import's.
/// </summary>
public static class EndingShades
{
    /// <summary>The levels every faded group moves through: a group at level L shows its colours scaled by L over this.</summary>
    public const int FadeLevels = EndingExplosionFadeDefinitions.Steps;

    /// <summary>
    /// The installed name of each colour load's shade (#2511), written out so renaming a load's identity never changes a name
    /// on disk.
    /// </summary>
    private static readonly FrozenDictionary<EndingColourLoad, string> LoadKeys =
        new Dictionary<EndingColourLoad, string>
        {
            [EndingColourLoad.Escape] = "EndingEscape",
            [EndingColourLoad.Explosion] = "EndingExplosion",
            [EndingColourLoad.ExplosionBackgrounds] = "EndingExplosionBackgrounds",
            [EndingColourLoad.FinalGunship] = "EndingFinalGunship",
            [EndingColourLoad.Credits] = "EndingCredits",
            [EndingColourLoad.PostCredits] = "EndingPostCredits",
            [EndingColourLoad.RestoredShot] = "EndingRestoredShot",
            [EndingColourLoad.LogoInitial] = "EndingLogoInitial",
        }.ToFrozenDictionary();

    /// <summary>
    /// The installed name of each faded group's shades (#2511), written out so renaming a group's identity never changes a
    /// name on disk.
    /// </summary>
    private static readonly FrozenDictionary<EndingFade, string> FadeKeys =
        new Dictionary<EndingFade, string>
        {
            [EndingFade.ExplosionBackground] = "EndingFadeExplosionBackground",
            [EndingFade.ExplosionLava] = "EndingFadeExplosionLava",
            [EndingFade.ExplodingPlanet] = "EndingFadeExplodingPlanet",
            [EndingFade.WaitingBackdrop] = "EndingFadeWaitingBackdrop",
            [EndingFade.SuitlessSamus] = "EndingFadeSuitlessSamus",
            [EndingFade.SuitedSamus] = "EndingFadeSuitedSamus",
            [EndingFade.Shot] = "EndingFadeShot",
            [EndingFade.ShootingSamus] = "EndingFadeShootingSamus",
        }.ToFrozenDictionary();

    /// <summary>
    /// The installed name of each flash's shade (#2511), written out so renaming a flash's identity never changes a name on
    /// disk.
    /// </summary>
    private static readonly FrozenDictionary<EndingFlash, string> FlashKeys =
        new Dictionary<EndingFlash, string>
        {
            [EndingFlash.PlanetGlow] = "EndingFlashPlanetGlow",
            [EndingFlash.SilhouetteBackdrop] = "EndingFlashSilhouetteBackdrop",
            [EndingFlash.ExplosionWhiteout] = "EndingFlashExplosionWhiteout",
            [EndingFlash.WhiteoutCleared] = "EndingFlashWhiteoutCleared",
            [EndingFlash.LogoBackground] = "EndingFlashLogoBackground",
        }.ToFrozenDictionary();

    /// <summary>
    /// The installed name of each actor colouring's drawn shade (#2511), written out so renaming a colouring's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<EndingSpriteColouring, string> DrawnKeys =
        new Dictionary<EndingSpriteColouring, string>
        {
            [EndingSpriteColouring.ClearTimeDigits] = "EndingDrawnClearTimeDigits",
            [EndingSpriteColouring.ClearTimeLabel] = "EndingDrawnClearTimeLabel",
            [EndingSpriteColouring.CompletionText] = "EndingDrawnCompletionText",
            [EndingSpriteColouring.Clouds] = "EndingDrawnClouds",
            [EndingSpriteColouring.ExplosionLava] = "EndingDrawnExplosionLava",
            [EndingSpriteColouring.ExplodingPlanet] = "EndingDrawnExplodingPlanet",
            [EndingSpriteColouring.ExplosionAfterglow] = "EndingDrawnExplosionAfterglow",
            [EndingSpriteColouring.AnimalEscape] = "EndingDrawnAnimalEscape",
            [EndingSpriteColouring.Logo] = "EndingDrawnLogo",
            [EndingSpriteColouring.SuitlessSamus] = "EndingDrawnSuitlessSamus",
            [EndingSpriteColouring.SuitedSamus] = "EndingDrawnSuitedSamus",
            [EndingSpriteColouring.FallingSamus] = "EndingDrawnFallingSamus",
            [EndingSpriteColouring.ShootingSamus] = "EndingDrawnShootingSamus",
            [EndingSpriteColouring.ShootingStars] = "EndingDrawnShootingStars",
        }.ToFrozenDictionary();

    /// <summary>The colours <paramref name="load"/> shows over those it replaces.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The load has no installed name.</exception>
    public static PictureShade Loaded(EndingColourLoad load) => new(Ensure.Listed(LoadKeys, load), 0);

    /// <summary>The colours of <paramref name="colours"/> at fade <paramref name="level"/> of <see cref="FadeLevels"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The group has no installed name.</exception>
    public static PictureShade Faded(EndingFade colours, int level) => new(Ensure.Listed(FadeKeys, colours), level);

    /// <summary>The colours <paramref name="flash"/> covers, in its solid colour.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The flash has no installed name.</exception>
    public static PictureShade Flashed(EndingFlash flash) => new(Ensure.Listed(FlashKeys, flash), 0);

    /// <summary>The logo's background and sprite colours at crossfade <paramref name="step"/>.</summary>
    public static PictureShade LogoCrossfade(EndingLogoCrossfadeStep step) => new("EndingLogoCrossfade", step.Step);

    /// <summary>An actor's recolourable parts drawn in <paramref name="colouring"/>, in the sheet's own colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring has no installed name.</exception>
    public static PictureShade Drawn(EndingSpriteColouring colouring) => new(Ensure.Listed(DrawnKeys, colouring), 0);

    /// <summary>
    /// What a screen shade <paramref name="shown"/> authored for the actors drawn in <paramref name="drawn"/> gives their
    /// pictures (#1544): the colours it shows in that colouring's colours over the inks of their parts that take the colouring, and
    /// over their own-colour parts' inks in those parts' own colours.
    /// </summary>
    public static PictureShade Under(PictureShade drawn, PictureShade shown) => new($"{drawn.Effect}.{shown.Effect}", shown.Step);
}

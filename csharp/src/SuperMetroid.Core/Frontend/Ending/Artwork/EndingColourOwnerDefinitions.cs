using System.Collections.Frozen;
using System.Collections.Immutable;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The ending actors and scenery each of the ending's colour effects is authored for (#1544): an actor's pictures show an
/// effect only when its colouring is listed for it, and a background picture or floor only when it is listed, so nothing
/// shows an effect authored for another, even where both once drew in one colour group (the animals' escape pod and the
/// exploding planet, the logo and Samus shooting, a floor and the sprites over it): that sharing was a presentation quirk,
/// and each shows its own colours. A scene's whole screen of colours is every actor's and every scenery's own. Each listing
/// follows the effect's documented subject and the scene that shows it: the planet's cue starts its fade-out and darkens its
/// glow, the flyaway's setup starts the afterglow the finale spawned and the gunship's emergence, the logo's landing starts
/// its glare; the first escape scene runs the lava and crust effects over its floor and the second the grey clouds over its
/// own, the explosion's crossfade fades the escape's far background out of the planet's floor, its burst and finale run
/// their effects over the explosion's pictures, the whiteout's floor colour is the flyaway floor's, and the post-credits
/// shot's floor is drawn in the shot's colours. No ending frame keeps its own colours in another actor's colour group but
/// the animals' escape pod's: every installed cinematic frame inherits its actor's colours, and
/// the shooting stars keep their own.
/// </summary>
internal static class EndingColourOwnerDefinitions
{
    /// <summary>Every actor: a scene's whole screen of colours.</summary>
    private static readonly ImmutableArray<EndingSpriteColouring> EveryActor =
    [
        EndingSpriteColouring.ClearTimeDigits, EndingSpriteColouring.ClearTimeLabel, EndingSpriteColouring.CompletionText,
        EndingSpriteColouring.Clouds, EndingSpriteColouring.ExplosionLava, EndingSpriteColouring.ExplodingPlanet,
        EndingSpriteColouring.ExplosionAfterglow, EndingSpriteColouring.AnimalEscape, EndingSpriteColouring.Logo,
        EndingSpriteColouring.SuitlessSamus, EndingSpriteColouring.SuitedSamus, EndingSpriteColouring.FallingSamus,
        EndingSpriteColouring.ShootingSamus, EndingSpriteColouring.ShootingStars,
    ];

    /// <summary>Every background picture and floor: a scene's whole screen of colours.</summary>
    private static readonly ImmutableArray<EndingScenery> EveryScenery =
    [
        EndingScenery.EscapeSceneAFloor, EndingScenery.EscapeSceneBFloor, EndingScenery.PlanetExplosionFloor,
        EndingScenery.FlyawayFloor, EndingScenery.PostCreditsShotFloor, EndingScenery.ExplosionPictures,
        EndingScenery.CreditsRows, EndingScenery.WaitingBackdrop, EndingScenery.WavySamus, EndingScenery.PostCreditsText,
        EndingScenery.LogoPicture,
    ];

    /// <summary>A scene's whole screen of colours, every actor's and every scenery's own.</summary>
    private static readonly EndingEffectOwners WholeScreen = new(EveryActor, EveryScenery);

    /// <summary>What each colour load is authored for.</summary>
    internal static readonly FrozenDictionary<EndingColourLoad, EndingEffectOwners> Loads =
        new Dictionary<EndingColourLoad, EndingEffectOwners>
        {
            [EndingColourLoad.Escape] = WholeScreen,
            [EndingColourLoad.Explosion] = WholeScreen,
            [EndingColourLoad.ExplosionBackgrounds] = new([], [EndingScenery.PlanetExplosionFloor, EndingScenery.ExplosionPictures,
                EndingScenery.FlyawayFloor]),
            [EndingColourLoad.FinalGunship] = new([], [EndingScenery.FlyawayFloor]),
            [EndingColourLoad.Credits] = WholeScreen,
            [EndingColourLoad.PostCredits] = WholeScreen,
            [EndingColourLoad.RestoredShot] = new([EndingSpriteColouring.ShootingStars], [EndingScenery.PostCreditsShotFloor]),
            [EndingColourLoad.LogoInitial] = new([EndingSpriteColouring.Logo], []),
        }.ToFrozenDictionary();

    /// <summary>
    /// What each load within a scene restores (#1597): restoring the explosion's backgrounds as its hold begins ends the
    /// far background's fade-out, the final gunship's colours replace the look the gunship's emergence settled on, and the
    /// restored shot ends the shot's fade-out. The assembling logo's first colours restore no effect's colours.
    /// </summary>
    internal static readonly FrozenDictionary<EndingColourLoad, EndingRestoredEffects> Restores =
        new Dictionary<EndingColourLoad, EndingRestoredEffects>
        {
            [EndingColourLoad.ExplosionBackgrounds] = new([EndingFade.ExplosionBackground], []),
            [EndingColourLoad.FinalGunship] = new([], [ColourEffectColours.GunshipEmergingFromExplosion]),
            [EndingColourLoad.RestoredShot] = new([EndingFade.Shot], []),
            [EndingColourLoad.LogoInitial] = new([], []),
        }.ToFrozenDictionary();

    /// <summary>What each faded group is authored for.</summary>
    internal static readonly FrozenDictionary<EndingFade, EndingEffectOwners> Fades =
        new Dictionary<EndingFade, EndingEffectOwners>
        {
            [EndingFade.ExplosionBackground] = new([], [EndingScenery.PlanetExplosionFloor]),
            [EndingFade.ExplosionLava] = new([EndingSpriteColouring.ExplosionLava], []),
            [EndingFade.ExplodingPlanet] = new([EndingSpriteColouring.ExplodingPlanet], []),
            [EndingFade.WaitingBackdrop] = new([], [EndingScenery.WaitingBackdrop]),
            [EndingFade.SuitlessSamus] = new([EndingSpriteColouring.SuitlessSamus], []),
            [EndingFade.SuitedSamus] = new([EndingSpriteColouring.SuitedSamus], []),
            [EndingFade.Shot] = new([EndingSpriteColouring.ShootingStars], [EndingScenery.PostCreditsShotFloor]),
            [EndingFade.ShootingSamus] = new([EndingSpriteColouring.ShootingSamus], []),
        }.ToFrozenDictionary();

    /// <summary>What each flash is authored for.</summary>
    internal static readonly FrozenDictionary<EndingFlash, EndingEffectOwners> Flashes =
        new Dictionary<EndingFlash, EndingEffectOwners>
        {
            [EndingFlash.PlanetGlow] = new([EndingSpriteColouring.ExplodingPlanet], []),
            [EndingFlash.SilhouetteBackdrop] = new([], []),
            [EndingFlash.ExplosionWhiteout] = new([], [EndingScenery.FlyawayFloor]),
            [EndingFlash.WhiteoutCleared] = new([], [EndingScenery.FlyawayFloor]),
            [EndingFlash.LogoBackground] = new([], [EndingScenery.LogoPicture]),
        }.ToFrozenDictionary();

    /// <summary>What the logo crossfade is authored for.</summary>
    internal static readonly EndingEffectOwners LogoCrossfade = new([EndingSpriteColouring.Logo], [EndingScenery.LogoPicture]);

    /// <summary>What each palette effect's colours the ending shows are authored for.</summary>
    internal static readonly FrozenDictionary<ColourEffectColours, EndingEffectOwners> ColourEffects =
        new Dictionary<ColourEffectColours, EndingEffectOwners>
        {
            [ColourEffectColours.WideZebesExplosionForeground] = new([], [EndingScenery.ExplosionPictures]),
            [ColourEffectColours.WideZebesExplosionBackground] = new([], [EndingScenery.ExplosionPictures]),
            [ColourEffectColours.ZebesExplosionFinale] = new([], [EndingScenery.ExplosionPictures]),
            [ColourEffectColours.SpaceDuringZebesExplosion] = new([], [EndingScenery.ExplosionPictures]),
            [ColourEffectColours.ExplodingZebesLava] = new([], [EndingScenery.EscapeSceneAFloor]),
            [ColourEffectColours.ExplodingZebesCrust] = new([], [EndingScenery.EscapeSceneAFloor]),
            [ColourEffectColours.ExplodingZebesGreyClouds] = new([], [EndingScenery.EscapeSceneBFloor]),
            [ColourEffectColours.GunshipEmergingFromExplosion] = new([], [EndingScenery.FlyawayFloor]),
            [ColourEffectColours.ZebesPlanetAfterglow] = new([EndingSpriteColouring.ExplosionAfterglow], []),
            [ColourEffectColours.ZoomedOutExplodingZebes] = new([EndingSpriteColouring.ExplodingPlanet], []),
            [ColourEffectColours.PostCreditsSuperMetroidIcon] = new([EndingSpriteColouring.Logo], []),
        }.ToFrozenDictionary();
}

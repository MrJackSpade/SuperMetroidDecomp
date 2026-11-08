using System.Collections.Frozen;
using System.Collections.Immutable;
using SuperMetroid.Core.Fx.PaletteFx;

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
/// the animals' escape pod's: every cinematic frame the import reads from the cartridge inherits its actor's colours, and
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

    /// <summary>The owners of an effect authored for the actors drawn in <paramref name="actors"/> alone.</summary>
    private static EndingEffectOwners Actors(ImmutableArray<EndingSpriteColouring> actors) => new(actors, []);

    /// <summary>The owners of an effect authored for <paramref name="scenery"/> alone.</summary>
    private static EndingEffectOwners Scenery(ImmutableArray<EndingScenery> scenery) => new([], scenery);

    /// <summary>What each colour load is authored for.</summary>
    internal static readonly FrozenDictionary<EndingColourLoad, EndingEffectOwners> Loads =
        new Dictionary<EndingColourLoad, EndingEffectOwners>
        {
            [EndingColourLoad.Escape] = WholeScreen,
            [EndingColourLoad.Explosion] = WholeScreen,
            [EndingColourLoad.ExplosionBackgrounds] = Scenery([EndingScenery.PlanetExplosionFloor, EndingScenery.ExplosionPictures,
                EndingScenery.FlyawayFloor]),
            [EndingColourLoad.FinalGunship] = Scenery([EndingScenery.FlyawayFloor]),
            [EndingColourLoad.Credits] = WholeScreen,
            [EndingColourLoad.PostCredits] = WholeScreen,
            [EndingColourLoad.RestoredShot] = new([EndingSpriteColouring.ShootingStars], [EndingScenery.PostCreditsShotFloor]),
            [EndingColourLoad.LogoInitial] = Actors([EndingSpriteColouring.Logo]),
        }.ToFrozenDictionary();

    /// <summary>What each faded group is authored for.</summary>
    internal static readonly FrozenDictionary<EndingFade, EndingEffectOwners> Fades =
        new Dictionary<EndingFade, EndingEffectOwners>
        {
            [EndingFade.ExplosionBackground] = Scenery([EndingScenery.PlanetExplosionFloor]),
            [EndingFade.ExplosionLava] = Actors([EndingSpriteColouring.ExplosionLava]),
            [EndingFade.ExplodingPlanet] = Actors([EndingSpriteColouring.ExplodingPlanet]),
            [EndingFade.WaitingBackdrop] = Scenery([EndingScenery.WaitingBackdrop]),
            [EndingFade.SuitlessSamus] = Actors([EndingSpriteColouring.SuitlessSamus]),
            [EndingFade.SuitedSamus] = Actors([EndingSpriteColouring.SuitedSamus]),
            [EndingFade.Shot] = new([EndingSpriteColouring.ShootingStars], [EndingScenery.PostCreditsShotFloor]),
            [EndingFade.ShootingSamus] = Actors([EndingSpriteColouring.ShootingSamus]),
        }.ToFrozenDictionary();

    /// <summary>What each flash is authored for.</summary>
    internal static readonly FrozenDictionary<EndingFlash, EndingEffectOwners> Flashes =
        new Dictionary<EndingFlash, EndingEffectOwners>
        {
            [EndingFlash.PlanetGlow] = Actors([EndingSpriteColouring.ExplodingPlanet]),
            [EndingFlash.SilhouetteBackdrop] = Scenery([]),
            [EndingFlash.ExplosionWhiteout] = Scenery([EndingScenery.FlyawayFloor]),
            [EndingFlash.WhiteoutCleared] = Scenery([EndingScenery.FlyawayFloor]),
            [EndingFlash.LogoBackground] = Scenery([EndingScenery.LogoPicture]),
        }.ToFrozenDictionary();

    /// <summary>What the logo crossfade is authored for.</summary>
    internal static readonly EndingEffectOwners LogoCrossfade = new([EndingSpriteColouring.Logo], [EndingScenery.LogoPicture]);

    /// <summary>What each palette effect's colours the ending shows are authored for.</summary>
    internal static readonly FrozenDictionary<PaletteFxColours, EndingEffectOwners> PaletteFx =
        new Dictionary<PaletteFxColours, EndingEffectOwners>
        {
            [PaletteFxColours.WideZebesExplosionForeground] = Scenery([EndingScenery.ExplosionPictures]),
            [PaletteFxColours.WideZebesExplosionBackground] = Scenery([EndingScenery.ExplosionPictures]),
            [PaletteFxColours.ZebesExplosionFinale] = Scenery([EndingScenery.ExplosionPictures]),
            [PaletteFxColours.SpaceDuringZebesExplosion] = Scenery([EndingScenery.ExplosionPictures]),
            [PaletteFxColours.ExplodingZebesLava] = Scenery([EndingScenery.EscapeSceneAFloor]),
            [PaletteFxColours.ExplodingZebesCrust] = Scenery([EndingScenery.EscapeSceneAFloor]),
            [PaletteFxColours.ExplodingZebesGreyClouds] = Scenery([EndingScenery.EscapeSceneBFloor]),
            [PaletteFxColours.GunshipEmergingFromExplosion] = Scenery([EndingScenery.FlyawayFloor]),
            [PaletteFxColours.ZebesPlanetAfterglow] = Actors([EndingSpriteColouring.ExplosionAfterglow]),
            [PaletteFxColours.ZoomedOutExplodingZebes] = Actors([EndingSpriteColouring.ExplodingPlanet]),
            [PaletteFxColours.PostCreditsSuperMetroidIcon] = Actors([EndingSpriteColouring.Logo]),
        }.ToFrozenDictionary();
}

using System.Collections.Frozen;
using System.Collections.Immutable;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The ending actors each of the ending's colour effects is authored for (#1544), by the colouring they draw in: an actor's
/// pictures show an effect only when its colouring is listed for it, so no actor shows an effect authored for another,
/// even where both once drew in one colour group (the animals' escape pod and the exploding planet, the logo and Samus
/// shooting): that sharing was a presentation quirk, and each actor now shows its own colours. A scene's whole screen of colours is every actor's own; an effect that changes only background or floor
/// colours is no actor's. Each actor's listing follows the effect's documented subject and the actor or cue that starts it:
/// the planet's cue starts its fade-out and darkens its glow, the flyaway's setup starts the afterglow the finale spawned, the
/// logo's landing starts its glare.
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

    /// <summary>No actor: an effect on background or floor colours alone.</summary>
    private static readonly ImmutableArray<EndingSpriteColouring> NoActor = [];

    /// <summary>The actors each colour load is authored for.</summary>
    internal static readonly FrozenDictionary<EndingColourLoad, ImmutableArray<EndingSpriteColouring>> Loads =
        new Dictionary<EndingColourLoad, ImmutableArray<EndingSpriteColouring>>
        {
            [EndingColourLoad.Escape] = EveryActor,
            [EndingColourLoad.Explosion] = EveryActor,
            [EndingColourLoad.ExplosionBackgrounds] = NoActor,
            [EndingColourLoad.FinalGunship] = NoActor,
            [EndingColourLoad.Credits] = EveryActor,
            [EndingColourLoad.PostCredits] = EveryActor,
            [EndingColourLoad.RestoredShot] = [EndingSpriteColouring.ShootingStars],
            [EndingColourLoad.LogoInitial] = [EndingSpriteColouring.Logo],
        }.ToFrozenDictionary();

    /// <summary>The actors each faded group is authored for.</summary>
    internal static readonly FrozenDictionary<EndingFade, ImmutableArray<EndingSpriteColouring>> Fades =
        new Dictionary<EndingFade, ImmutableArray<EndingSpriteColouring>>
        {
            [EndingFade.ExplosionBackground] = NoActor,
            [EndingFade.ExplosionLava] = [EndingSpriteColouring.ExplosionLava],
            [EndingFade.ExplodingPlanet] = [EndingSpriteColouring.ExplodingPlanet],
            [EndingFade.WaitingBackdrop] = NoActor,
            [EndingFade.SuitlessSamus] = [EndingSpriteColouring.SuitlessSamus],
            [EndingFade.SuitedSamus] = [EndingSpriteColouring.SuitedSamus],
            [EndingFade.Shot] = [EndingSpriteColouring.ShootingStars],
            [EndingFade.ShootingSamus] = [EndingSpriteColouring.ShootingSamus],
        }.ToFrozenDictionary();

    /// <summary>The actors each flash is authored for.</summary>
    internal static readonly FrozenDictionary<EndingFlash, ImmutableArray<EndingSpriteColouring>> Flashes =
        new Dictionary<EndingFlash, ImmutableArray<EndingSpriteColouring>>
        {
            [EndingFlash.PlanetGlow] = [EndingSpriteColouring.ExplodingPlanet],
            [EndingFlash.SilhouetteBackdrop] = NoActor,
            [EndingFlash.ExplosionWhiteout] = NoActor,
            [EndingFlash.WhiteoutCleared] = NoActor,
            [EndingFlash.LogoBackground] = NoActor,
        }.ToFrozenDictionary();

    /// <summary>The actors the logo crossfade is authored for.</summary>
    internal static readonly ImmutableArray<EndingSpriteColouring> LogoCrossfade = [EndingSpriteColouring.Logo];

    /// <summary>The actors each palette effect's colours the ending shows are authored for.</summary>
    internal static readonly FrozenDictionary<PaletteFxColours, ImmutableArray<EndingSpriteColouring>> PaletteFx =
        new Dictionary<PaletteFxColours, ImmutableArray<EndingSpriteColouring>>
        {
            [PaletteFxColours.WideZebesExplosionForeground] = NoActor,
            [PaletteFxColours.WideZebesExplosionBackground] = NoActor,
            [PaletteFxColours.ZebesExplosionFinale] = NoActor,
            [PaletteFxColours.SpaceDuringZebesExplosion] = NoActor,
            [PaletteFxColours.ExplodingZebesLava] = NoActor,
            [PaletteFxColours.ExplodingZebesCrust] = NoActor,
            [PaletteFxColours.ExplodingZebesGreyClouds] = NoActor,
            [PaletteFxColours.GunshipEmergingFromExplosion] = NoActor,
            [PaletteFxColours.ZebesPlanetAfterglow] = [EndingSpriteColouring.ExplosionAfterglow],
            [PaletteFxColours.ZoomedOutExplodingZebes] = [EndingSpriteColouring.ExplodingPlanet],
            [PaletteFxColours.PostCreditsSuperMetroidIcon] = [EndingSpriteColouring.Logo],
        }.ToFrozenDictionary();
}

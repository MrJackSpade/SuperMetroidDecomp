using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The order the ending's colours lay the shades of one kind over each other (#2543), written out so reordering a kind's
/// members never changes the picture: within the loads, the fades and the flashes the running scene shows, each shade lies
/// over those listed before it.
/// </summary>
internal static partial class EndingShadeLayeringDefinitions
{
    /// <summary>The loads within a scene that restore some of its colours, from the lowest shade to the highest.</summary>
    internal static IReadOnlyList<EndingColourLoad> Loads { get; } =
    [
        EndingColourLoad.Escape,
        EndingColourLoad.Explosion,
        EndingColourLoad.ExplosionBackgrounds,
        EndingColourLoad.FinalGunship,
        EndingColourLoad.Credits,
        EndingColourLoad.PostCredits,
        EndingColourLoad.RestoredShot,
        EndingColourLoad.LogoInitial,
    ];

    /// <summary>The fades, from the lowest shade to the highest.</summary>
    internal static IReadOnlyList<EndingFade> Fades { get; } =
    [
        EndingFade.ExplosionBackground,
        EndingFade.ExplosionLava,
        EndingFade.ExplodingPlanet,
        EndingFade.WaitingBackdrop,
        EndingFade.SuitlessSamus,
        EndingFade.SuitedSamus,
        EndingFade.Shot,
        EndingFade.ShootingSamus,
    ];

    /// <summary>The flashes, from the lowest shade to the highest.</summary>
    internal static IReadOnlyList<EndingFlash> Flashes { get; } =
    [
        EndingFlash.PlanetGlow,
        EndingFlash.SilhouetteBackdrop,
        EndingFlash.ExplosionWhiteout,
        EndingFlash.WhiteoutCleared,
        EndingFlash.LogoBackground,
    ];
}

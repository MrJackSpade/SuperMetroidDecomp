namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Where the ending's actors stand in the order they run and draw.</summary>
internal static class EndingCastOrderDefinitions
{
    /// <summary>
    /// The actors the ending shows one of at a time, in the order they run and draw after every other actor: the
    /// afterglow, the animals' pod, the left starfield and last the right starfield. A new one replaces the one already
    /// shown; every other actor runs and draws in the order it spawned, ahead of these.
    /// </summary>
    internal static readonly EndingSpriteRole[] SoleActorsLast =
    [
        EndingSpriteRole.ExplosionAfterglow,
        EndingSpriteRole.AnimalEscape,
        EndingSpriteRole.ExplosionStarsLeft,
        EndingSpriteRole.ExplosionStarsRight,
    ];
}

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// How one of the Tourian unlock's effects moves itself (#1843).
/// </summary>
internal enum TourianStatueEffectMotion
{
    /// <summary>Holds a Tourian statue splash at the water surface.</summary>
    SplashHolding,

    /// <summary>Drops a Tourian statue particle, splashing as it crosses the water.</summary>
    ParticleFalling,

    /// <summary>Raises a Tourian statue soul out of the room.</summary>
    SoulRising,
}

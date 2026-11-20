namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// How one of Phantoon's flames that circle his body moves itself (#1843): an intro, rage or spiral flame.
/// </summary>
internal enum PhantoonFlameMotion
{
    /// <summary>Holds an intro flame until Phantoon's body activates it.</summary>
    IntroWaiting,

    /// <summary>Orbits an intro flame while its radius contracts.</summary>
    IntroOrbiting,

    /// <summary>Orbits a rage flame outward around Phantoon's body.</summary>
    RageOrbiting,

    /// <summary>Spirals a flame outward around Phantoon's body.</summary>
    SpiralOrbiting,
}

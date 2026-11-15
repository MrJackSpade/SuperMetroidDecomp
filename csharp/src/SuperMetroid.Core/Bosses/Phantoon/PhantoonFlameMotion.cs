namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// How one of Phantoon's flames moves itself (#1843).
/// </summary>
internal enum PhantoonFlameMotion
{
    /// <summary>Holds an intro flame until Phantoon's body activates it.</summary>
    IntroWaiting,

    /// <summary>Orbits an intro flame while its radius contracts.</summary>
    IntroOrbiting,

    /// <summary>Drops a casual flame until it first strikes the room.</summary>
    CasualFalling,

    /// <summary>Holds a landed casual flame for its impact pause.</summary>
    CasualLanding,

    /// <summary>Bounces a casual flame twice, each bounce lower.</summary>
    CasualBouncing,

    /// <summary>Orbits a rage flame outward around Phantoon's body.</summary>
    RageOrbiting,

    /// <summary>Drops a rain flame after its delay until it strikes the room.</summary>
    RainFalling,

    /// <summary>Spirals a flame outward around Phantoon's body.</summary>
    SpiralOrbiting,
}

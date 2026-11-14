namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The behaviour a Powamp body's main AI runs. The balloon half has none: its body moves it.
/// Its values are arbitrary.
/// </summary>
public enum PowampAction
{
    /// <summary>Holds the body at its resting height until its 60-update countdown starts balloon inflation.</summary>
    DeflatedResting,
    /// <summary>Waits ten AI updates, then selects fast body animation and upward velocity of half a pixel per update.</summary>
    Inflating,
    /// <summary>Rises with horizontal wiggle toward 64 pixels above the balloon's spawn Y, switching immediately to grappled rise if grapple AI activates.</summary>
    InflatedRiseToTargetHeight,
    /// <summary>Completes the wiggle to a centered phase before deflating, continuing terrain-clipped upward movement while off center.</summary>
    InflatedFinishWiggle,
    /// <summary>Rises toward the population-defined grapple height while attached, switching to ordinary finish-wiggle without moving on the release update.</summary>
    GrappledRiseToTargetHeight,
    /// <summary>Centers the wiggle before resting at the grapple target, or hands off to ordinary finish-wiggle when released.</summary>
    GrappledFinishWiggle,
    /// <summary>Holds the raised body while grapple AI remains active and starts deflation on release.</summary>
    GrappledResting,
    /// <summary>Waits ten AI updates for balloon deflation, then selects downward velocity of one pixel per update.</summary>
    Deflating,
    /// <summary>Sinks with terrain collision handling until the body reaches the balloon's spawn Y, then restores slow animation and the rest countdown.</summary>
    DeflatedSinking,
    /// <summary>Adjusts the balloon's inflation animation and initializes the 32-update death delay after a fatal shot.</summary>
    FatalDamage,
    /// <summary>Maintains balloon alignment during the countdown, then deletes both halves and fires spikes in eight directions.</summary>
    DeathSequence,
}

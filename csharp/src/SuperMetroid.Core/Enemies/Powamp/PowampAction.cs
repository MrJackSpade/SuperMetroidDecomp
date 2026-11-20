namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The behaviour a Powamp body's main AI runs. The balloon half has none: its body moves it.
/// Its values are arbitrary.
/// </summary>
public enum PowampAction
{
    /// <summary>Holds the body at its resting height until its rest (<see cref="PowampMotionDefinitions.RestDuration"/>) runs out and starts balloon inflation.</summary>
    DeflatedResting,
    /// <summary>Waits out its inflation (<see cref="PowampMotionDefinitions.InflationDuration"/>), then selects fast body animation and its upward velocity (<see cref="PowampMotionDefinitions.RiseVelocity"/>).</summary>
    Inflating,
    /// <summary>Rises with horizontal wiggle toward its height above the balloon's spawn Y (<see cref="PowampMotionDefinitions.UngrappledRise"/>), switching immediately to grappled rise once the grapple beam holds it.</summary>
    InflatedRiseToTargetHeight,
    /// <summary>Completes the wiggle to a centered phase before deflating, continuing terrain-clipped upward movement while off center.</summary>
    InflatedFinishWiggle,
    /// <summary>Rises toward the population-defined grapple height while attached, switching to ordinary finish-wiggle without moving on the release update.</summary>
    GrappledRiseToTargetHeight,
    /// <summary>Centers the wiggle before resting at the grapple target, or hands off to ordinary finish-wiggle when released.</summary>
    GrappledFinishWiggle,
    /// <summary>Holds the raised body while the grapple beam holds it and starts deflation on release.</summary>
    GrappledResting,
    /// <summary>Waits out the balloon's deflation (<see cref="PowampMotionDefinitions.InflationDuration"/>), then selects its downward velocity (<see cref="PowampMotionDefinitions.SinkVelocity"/>).</summary>
    Deflating,
    /// <summary>Sinks with terrain collision handling until the body reaches the balloon's spawn Y, then restores slow animation and the rest countdown.</summary>
    DeflatedSinking,
    /// <summary>Adjusts the balloon's inflation animation and starts the death delay (<see cref="PowampMotionDefinitions.DeathDelay"/>) after a fatal shot.</summary>
    FatalDamage,
    /// <summary>Maintains balloon alignment during the countdown, then deletes both halves and fires spikes in eight directions.</summary>
    DeathSequence,
}

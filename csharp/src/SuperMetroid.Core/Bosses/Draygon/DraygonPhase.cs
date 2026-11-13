namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The phase of Draygon's fight, the behaviour his body runs each frame. Each member names that behaviour, and the
/// body's state stores the member.
/// </summary>
public enum DraygonPhase
{
    /// <summary>A cleared record is in no phase.</summary>
    None = 0,
    /// <summary>Loads four Evir sprites, then waits 256 gameplay frames.</summary>
    IntroInitialDelay = 1,
    /// <summary>Advances the opening Evir movement stream for 1,232 frames before the first swoop.</summary>
    IntroDance,
    /// <summary>Constructs the swoop-height table, aims horizontal velocity at Samus, and installs right-facing graphics.</summary>
    SwoopRightSetup,
    /// <summary>Traverses height samples backward while moving right toward the bottom of the swoop.</summary>
    SwoopRightDescending,
    /// <summary>Recalculates horizontal velocity toward the right exit for the ascending half.</summary>
    SwoopRightApex,
    /// <summary>Traverses height samples forward and selects a leftward swoop or goop pass from the current random number.</summary>
    SwoopRightAscending,
    /// <summary>Reuses the generated height table, aims leftward velocity at Samus, and installs left-facing graphics.</summary>
    SwoopLeftSetup,
    /// <summary>Traverses height samples backward while moving left toward the bottom of the swoop.</summary>
    SwoopLeftDescending,
    /// <summary>Recalculates horizontal velocity toward the left reset position for the ascending half.</summary>
    SwoopLeftApex,
    /// <summary>Completes the leftward ascent, resets X, and selects a rightward swoop or goop pass from the current random number.</summary>
    SwoopLeftAscending,
    /// <summary>Starts the one-pixel-per-frame rightward goop path at X=-80, Y=384.</summary>
    GoopRightSetup,
    /// <summary>Follows the oscillating path until horizontal separation from Samus is below 208 pixels.</summary>
    GoopRight,
    /// <summary>Requests right-facing goop animations from RNG samples and begins chasing when goop slows Samus.</summary>
    GoopRightTail,
    /// <summary>Finishes the rightward oscillating exit without firing goop.</summary>
    GoopRightRecovery,
    /// <summary>Starts the one-pixel-per-frame leftward goop path at the right reset X and Y=384.</summary>
    GoopLeftSetup,
    /// <summary>Follows the oscillating path until horizontal separation from Samus is below 208 pixels.</summary>
    GoopLeft,
    /// <summary>Requests left-facing goop animations from RNG samples and begins chasing when goop slows Samus.</summary>
    GoopLeftTail,
    /// <summary>Completes the leftward exit, still allowing attached goop to trigger a chase.</summary>
    GoopLeftRecovery,
    /// <summary>Moves toward goop-slowed Samus at two pixels per frame and checks the eight-pixel claw window.</summary>
    TryGrabSamus,
    /// <summary>Despite the managed name, drops an active grapple and enters release without grabbing Samus.</summary>
    GrabbedSamus,
    /// <summary>Carries Samus toward room pixel (256,384), then starts the rising spiral.</summary>
    CarrySamus,
    /// <summary>Expands and raises the carry orbit, with RNG-selected incidental tail whips.</summary>
    FlailWithSamus,
    /// <summary>Holds the grabbed pair together during a 64-frame whip, then resumes the spiral.</summary>
    TailWhipWithSamus,
    /// <summary>Starts the facing-specific four-repeat finishing tail-whip animation.</summary>
    FinalTailWhips,
    /// <summary>Keeps Samus attached until the tail animation changes the body's phase to release.</summary>
    FinalTailWhipsWait,
    /// <summary>Releases Samus, restores ordinary collision, and starts a tail flail before retreat.</summary>
    ReleaseSamus,
    /// <summary>Retreats upward four pixels per frame, then resets for a rightward swoop.</summary>
    FlyStraightUp,
    /// <summary>Drifts toward room pixel (256,480), then spawns burial Evirs and retires the arms and tail.</summary>
    Dying,
    /// <summary>Despite the managed name, waits 416 frames while Evirs approach before the body sinks.</summary>
    DyingSink,
    /// <summary>Sinks one pixel per frame to Y=576, scatters drops, deletes the remaining parts, and persists defeat.</summary>
    DyingFinish,
}

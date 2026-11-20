namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The phase of Draygon's fight, the behaviour his body runs each frame. Each member names that behaviour, and the
/// body's state stores the member.
/// </summary>
public enum DraygonPhase
{
    /// <summary>Loads the Evir sprites, then waits out the intro's delay (<see cref="DraygonArenaDefinitions.IntroUpdates"/>).</summary>
    IntroInitialDelay,
    /// <summary>Advances the opening Evir movement for its duration (<see cref="DraygonIntroDanceDefinitions.DurationFrames"/>) before the first swoop.</summary>
    IntroDance,
    /// <summary>
    /// Starts a swoop its pass's way (<see cref="DraygonEnemyState.PassDirection"/>): rightward it plans the swoop's path of
    /// heights, leftward it reuses the planned path; either aims horizontal velocity at Samus and faces Draygon that way.
    /// </summary>
    SwoopSetup,
    /// <summary>Follows the swoop's path down, from its last height to its first, while moving its pass's way toward the bottom of the swoop.</summary>
    SwoopDescending,
    /// <summary>Recalculates horizontal velocity for the ascending half: toward the right exit, or the left reset position.</summary>
    SwoopApex,
    /// <summary>
    /// Follows the swoop's path back up and selects a swoop or goop pass the other way from the current random number; the
    /// leftward ascent first resets X.
    /// </summary>
    SwoopAscending,
    /// <summary>Starts the goop path its pass's way: rightward from <see cref="DraygonArenaDefinitions.OffscreenLeftX"/>, leftward from the right reset X, along <see cref="DraygonArenaDefinitions.SwimLineY"/>.</summary>
    GoopSetup,
    /// <summary>Follows the oscillating path until horizontal separation from Samus is below <see cref="DraygonArenaDefinitions.GoopTailRange"/>.</summary>
    GoopApproach,
    /// <summary>Requests goop animations facing its pass's way from RNG samples and begins chasing when goop slows Samus.</summary>
    GoopTail,
    /// <summary>Finishes the oscillating exit without firing goop; leftward, attached goop can still trigger a chase.</summary>
    GoopRecovery,
    /// <summary>Moves toward goop-slowed Samus at <see cref="DraygonGrabDefinitions.ChaseSpeed"/> and checks the claw window (<see cref="DraygonArenaDefinitions.GrabWindow"/>).</summary>
    TryGrabSamus,
    /// <summary>Repels a grapple beam that caught Draygon as he reached Samus: drops the beam and releases her without a grab (#1939).</summary>
    RepelGrapple,
    /// <summary>Carries Samus toward the spiral's target (<see cref="DraygonGrabDefinitions.SpiralTarget"/>), then starts the rising spiral.</summary>
    CarrySamus,
    /// <summary>Expands and raises the carry orbit, with RNG-selected incidental tail whips.</summary>
    FlailWithSamus,
    /// <summary>
    /// Starts the pause a tail whip makes in the spiral (<see cref="DraygonGrabDefinitions.TailWhipUpdates"/>) on its
    /// first update, holding the grabbed pair together, and starts the tail's whip.
    /// </summary>
    BeginTailWhipWithSamus,
    /// <summary>Holds the grabbed pair together through the rest of the tail whip's pause, then resumes the spiral.</summary>
    TailWhipWithSamus,
    /// <summary>Starts the facing-specific four-repeat finishing tail-whip animation.</summary>
    FinalTailWhips,
    /// <summary>Keeps Samus attached until the tail animation changes the body's phase to release.</summary>
    FinalTailWhipsWait,
    /// <summary>Releases Samus, restores ordinary collision, and starts a tail flail before retreat.</summary>
    ReleaseSamus,
    /// <summary>Retreats upward at <see cref="DraygonArenaDefinitions.SwoopClimbOutSpeed"/>, then resets for a rightward swoop.</summary>
    FlyStraightUp,
    /// <summary>Drifts toward his burial point (<see cref="DraygonArenaDefinitions.DeathTargetX"/>, <see cref="DraygonArenaDefinitions.DeathTargetY"/>), then spawns burial Evirs and retires the arms and tail.</summary>
    Dying,
    /// <summary>Waits out the burial wait (<see cref="DraygonArenaDefinitions.BurialWaitUpdates"/>) while the burial Evirs approach, before the body sinks (#1939).</summary>
    DyingWaitForEvirs,
    /// <summary>Sinks to <see cref="DraygonArenaDefinitions.BurialFinishedY"/>, scatters drops, deletes the remaining parts, and persists defeat.</summary>
    DyingFinish,
}

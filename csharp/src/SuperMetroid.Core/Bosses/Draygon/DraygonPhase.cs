namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The phase of Draygon's fight, the behaviour his body runs each frame (#2554). Each case names that behaviour and carries
/// what it runs on: the swoop and goop phases the swoop path the last rightward swoop planned. The body's state stores it.
/// </summary>
public abstract record DraygonPhase
{
    /// <summary>Closes the hierarchy to the phases declared here.</summary>
    private DraygonPhase()
    {
    }

    /// <summary>Loads the Evir sprites, then waits out the intro's delay (<see cref="DraygonArenaDefinitions.IntroUpdates"/>).</summary>
    public sealed record IntroInitialDelay : DraygonPhase;

    /// <summary>Advances the opening Evir movement for its duration (<see cref="DraygonIntroDanceDefinitions.DurationFrames"/>) before the first swoop.</summary>
    public sealed record IntroDance : DraygonPhase;

    /// <summary>
    /// Starts a rightward swoop (<see cref="DraygonEnemyState.PassDirection"/>): plans the swoop's path of heights, aims
    /// horizontal velocity at Samus and faces Draygon right.
    /// </summary>
    public sealed record RightSwoopSetup : DraygonPhase;

    /// <summary>
    /// Starts a leftward swoop (<see cref="DraygonEnemyState.PassDirection"/>) on <paramref name="Path"/>, the path the last
    /// rightward swoop planned: aims horizontal velocity at Samus and faces Draygon left.
    /// </summary>
    /// <param name="Path">The swoop path the last rightward swoop planned.</param>
    internal sealed record LeftSwoopSetup(DraygonSwoopPath Path) : DraygonPhase;

    /// <summary>Follows the swoop's path down, from its last height to its first, while moving its pass's way toward the bottom of the swoop.</summary>
    /// <param name="Path">The swoop path the last rightward swoop planned, which the swoop flies and a pass hands on to the next leftward swoop.</param>
    internal sealed record SwoopDescending(DraygonSwoopPath Path) : DraygonPhase;

    /// <summary>Recalculates horizontal velocity for the ascending half: toward the right exit, or the left reset position.</summary>
    /// <param name="Path">The swoop path the last rightward swoop planned, which the swoop flies and a pass hands on to the next leftward swoop.</param>
    internal sealed record SwoopApex(DraygonSwoopPath Path) : DraygonPhase;

    /// <summary>
    /// Follows the swoop's path back up and selects a swoop or goop pass the other way from the current random number; the
    /// leftward ascent first resets X.
    /// </summary>
    /// <param name="Path">The swoop path the last rightward swoop planned, which the swoop flies and a pass hands on to the next leftward swoop.</param>
    internal sealed record SwoopAscending(DraygonSwoopPath Path) : DraygonPhase;

    /// <summary>Starts the goop path its pass's way: rightward from <see cref="DraygonArenaDefinitions.OffscreenLeftX"/>, leftward from the right reset X, along <see cref="DraygonArenaDefinitions.SwimLineY"/>.</summary>
    /// <param name="Path">The swoop path the last rightward swoop planned, which the swoop flies and a pass hands on to the next leftward swoop.</param>
    internal sealed record GoopSetup(DraygonSwoopPath Path) : DraygonPhase;

    /// <summary>Follows the oscillating path until horizontal separation from Samus is below <see cref="DraygonArenaDefinitions.GoopTailRange"/>.</summary>
    /// <param name="Path">The swoop path the last rightward swoop planned, which the swoop flies and a pass hands on to the next leftward swoop.</param>
    internal sealed record GoopApproach(DraygonSwoopPath Path) : DraygonPhase;

    /// <summary>Requests goop animations facing its pass's way from RNG samples and begins chasing when goop slows Samus.</summary>
    /// <param name="Path">The swoop path the last rightward swoop planned, which the swoop flies and a pass hands on to the next leftward swoop.</param>
    internal sealed record GoopTail(DraygonSwoopPath Path) : DraygonPhase;

    /// <summary>Finishes the oscillating exit without firing goop; leftward, attached goop can still trigger a chase.</summary>
    /// <param name="Path">The swoop path the last rightward swoop planned, which the swoop flies and a pass hands on to the next leftward swoop.</param>
    internal sealed record GoopRecovery(DraygonSwoopPath Path) : DraygonPhase;

    /// <summary>Moves toward goop-slowed Samus at <see cref="DraygonGrabDefinitions.ChaseSpeed"/> and checks the claw window (<see cref="DraygonArenaDefinitions.GrabWindow"/>).</summary>
    public sealed record TryGrabSamus : DraygonPhase;

    /// <summary>Repels a grapple beam that caught Draygon as he reached Samus: drops the beam and releases her without a grab (#1939).</summary>
    public sealed record RepelGrapple : DraygonPhase;

    /// <summary>Carries Samus toward the spiral's target (<see cref="DraygonGrabDefinitions.SpiralTarget"/>), then starts the rising spiral.</summary>
    public sealed record CarrySamus : DraygonPhase;

    /// <summary>Expands and raises the carry orbit, with RNG-selected incidental tail whips.</summary>
    public sealed record FlailWithSamus : DraygonPhase;

    /// <summary>
    /// Starts the pause a tail whip makes in the spiral (<see cref="DraygonGrabDefinitions.TailWhipUpdates"/>) on its
    /// first update, holding the grabbed pair together, and starts the tail's whip.
    /// </summary>
    public sealed record BeginTailWhipWithSamus : DraygonPhase;

    /// <summary>Holds the grabbed pair together through the rest of the tail whip's pause, then resumes the spiral.</summary>
    public sealed record TailWhipWithSamus : DraygonPhase;

    /// <summary>Starts the facing-specific four-repeat finishing tail-whip animation.</summary>
    public sealed record FinalTailWhips : DraygonPhase;

    /// <summary>Keeps Samus attached until the tail animation changes the body's phase to release.</summary>
    public sealed record FinalTailWhipsWait : DraygonPhase;

    /// <summary>Releases Samus, restores ordinary collision, and starts a tail flail before retreat.</summary>
    public sealed record ReleaseSamus : DraygonPhase;

    /// <summary>Retreats upward at <see cref="DraygonArenaDefinitions.SwoopClimbOutSpeed"/>, then resets for a rightward swoop.</summary>
    public sealed record FlyStraightUp : DraygonPhase;

    /// <summary>Drifts toward his burial point (<see cref="DraygonArenaDefinitions.DeathTargetX"/>, <see cref="DraygonArenaDefinitions.DeathTargetY"/>), then spawns burial Evirs and retires the arms and tail.</summary>
    public sealed record Dying : DraygonPhase;

    /// <summary>Waits out the burial wait (<see cref="DraygonArenaDefinitions.BurialWaitUpdates"/>) while the burial Evirs approach, before the body sinks (#1939).</summary>
    public sealed record DyingWaitForEvirs : DraygonPhase;

    /// <summary>Sinks to <see cref="DraygonArenaDefinitions.BurialFinishedY"/>, scatters drops, deletes the remaining parts, and persists defeat.</summary>
    public sealed record DyingFinish : DraygonPhase;
}

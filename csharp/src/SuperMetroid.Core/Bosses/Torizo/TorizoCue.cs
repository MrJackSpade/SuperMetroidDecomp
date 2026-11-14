namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's body animations: what it does, with the values it acts on.</summary>
internal abstract record TorizoCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TorizoCue()
    {
    }

    /// <summary>A branch, call or link to another body animation.</summary>
    /// <param name="Kind">What the branch tests or does.</param>
    /// <param name="Target">The animation a branch or call continues in from its start, or a link saves as the return point.</param>
    internal sealed record Branch(TorizoBranchKind Kind, TorizoAnimation Target) : TorizoCue;

    /// <summary>A two-way branch or call between two body animations.</summary>
    /// <param name="Kind">What the choice tests.</param>
    /// <param name="Target">The animation the choice continues in from its start when its test passes.</param>
    /// <param name="Alternate">The animation a two-way branch or call continues in otherwise.</param>
    internal sealed record Choice(TorizoChoiceKind Kind, TorizoAnimation Target, TorizoAnimation Alternate) : TorizoCue;

    /// <summary>
    /// Plays the run of steps the cue is in again, from that run's first step, while its condition holds (#1788): a jump's
    /// rising pose while it rises, Golden Torizo's drop while it is airborne, its open chest while its eggs remain.
    /// </summary>
    /// <param name="Kind">The condition the replay tests.</param>
    internal sealed record Replay(TorizoReplayKind Kind) : TorizoCue;

    /// <summary>Stands Bomb Torizo up or sits it down, moving it by a movement-table entry.</summary>
    /// <param name="Kind">Which way the posture changes.</param>
    /// <param name="Step">The posture step the change applies, one of <see cref="BombTorizoMovement.PostureStepCount"/> steps.</param>
    internal sealed record Posture(TorizoPostureKind Kind, int Step) : TorizoCue;

    /// <summary>One walking step of a Torizo, read from its walking table.</summary>
    /// <param name="Kind">Which Torizo walks.</param>
    /// <param name="Step">The walking step, one of the walking Torizo's twenty steps.</param>
    internal sealed record Walk(TorizoWalkKind Kind, int Step) : TorizoCue;

    /// <summary>Spawns an explosive swipe at Bomb Torizo's hand.</summary>
    /// <param name="Hand">Where the hand is relative to a left-facing Torizo; a right-facing Torizo mirrors it.</param>
    internal sealed record Swipe(Offset16<RoomPixels> Hand) : TorizoCue;

    /// <summary>
    /// Spawns the low-health explosions and flashes; the animation then waits forty updates.
    /// </summary>
    /// <param name="Site">Where the explosions burst.</param>
    internal sealed record LowHealthExplosions(BombTorizoExplosionSite Site) : TorizoCue;

    /// <summary>Selects the main-AI phase.</summary>
    /// <param name="Phase">The main-AI phase a <see cref="SetPhase"/> cue selects.</param>
    internal sealed record SetPhase(TorizoPhase Phase) : TorizoCue;

    /// <summary>Installs the body's frame behaviour.</summary>
    /// <param name="FrameBehavior">The frame behaviour a <see cref="SetFrameBehavior"/> cue installs.</param>
    internal sealed record SetFrameBehavior(TorizoFrameBehavior FrameBehavior) : TorizoCue;

    /// <summary>Shows a look of the body's artwork under its poses.</summary>
    /// <param name="Look">The look a <see cref="ShowArtwork"/> cue shows.</param>
    internal sealed record ShowArtwork(TorizoArtworkLook Look) : TorizoCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(TorizoCueKind Kind) : TorizoCue;
}

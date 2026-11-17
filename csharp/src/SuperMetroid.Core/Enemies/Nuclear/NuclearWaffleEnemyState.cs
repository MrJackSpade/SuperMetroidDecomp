using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The state of the Nuclear Waffle, with its projectile and effect-actor links. The seven articulated body links
/// alternate between enemy projectiles and effect actors; the waffle spawns them all at its room load and keeps them.
/// </summary>
public sealed class NuclearWaffleEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal NuclearWaffleEnemyState(NuclearWaffleAction action) => Action = action;

    /// <summary>Where the body's typed animation (#641) resumes.</summary>
    internal AnimationPlayback<NuclearWaffleSegment> Playback { get; set; }

    /// <summary>The current waiting or sweeping behaviour.</summary>
    public NuclearWaffleAction Action { get; internal set; }

    /// <summary>
    /// The AI updates left in the wait, the one the sweep starts on included. It starts at <see cref="WaitingTimerReset"/>, and
    /// the update that takes it to zero starts the sweep and reloads it (#2281).
    /// </summary>
    public int WaitingTimer { get; internal set; }

    /// <summary>
    /// Orbit radius in room pixels, from the spawn settings. Every link and the head sit on
    /// the circle of this radius around <see cref="Origin"/>.
    /// </summary>
    public int OrbitRadius { get; internal set; }

    /// <summary>Whether the sweep runs forward rather than backward, as the spawn settings' direction says.</summary>
    public bool SweepsForward { get; internal set; }

    /// <summary>
    /// The wait, in AI updates, reloaded into <see cref="WaitingTimer"/> at room load and whenever a wait expires: the spawn
    /// settings' wait time and the update it runs out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>).
    /// </summary>
    public int WaitingTimerReset { get; internal set; }

    /// <summary>
    /// The sweep angle: its whole step is the head's angle and its fraction carries between updates. It restarts at
    /// <see cref="SweepStartAngle"/> with no fraction and moves monotonically toward <see cref="SweepEndAngle"/> until the
    /// last damaging link reaches it, so it stays between 140 and 500 steps, counted on past the full turn rather than
    /// wrapping; the orbit reads only its direction within the turn.
    /// </summary>
    public NuclearWaffleSweepAngle SweepAngle { get; internal set; }

    /// <summary>The head's current angle: the whole step of <see cref="SweepAngle"/>.</summary>
    public NuclearWaffleSweepAngle CurrentAngle => SweepAngle.WholeStep;

    /// <summary>The start angle: 400 steps for the backward sweep or 240 for the forward one.</summary>
    public NuclearWaffleSweepAngle SweepStartAngle { get; internal set; }
    /// <summary>The finish angle: 240 steps for the backward sweep or 400 for the forward one; the last damaging link's clamp result determines sweep completion.</summary>
    public NuclearWaffleSweepAngle SweepEndAngle { get; internal set; }
    /// <summary>
    /// How fast the sweep angle turns each AI update: the spawn settings' angular speed for a forward sweep and its
    /// reversal for a backward one.
    /// </summary>
    public AngularSpeed AngularSpeed { get; internal set; }
    /// <summary>The arc origin: initial population position in whole room pixels, retained as the fixed orbit center.</summary>
    public Position16<RoomPixels> Origin { get; internal set; }
    /// <summary>The link spawn position: head position after applying the initial angle and radius, used as the whole-pixel spawn position for the four projectile links.</summary>
    public Position16<RoomPixels> InitialHead { get; internal set; }
    /// <summary>The steps between one link and the next of the same kind, subtracted from each link's angle in turn: -24 for the backward sweep, whose links trail at larger angles, or 24 for the forward.</summary>
    public int SegmentSpacing { get; internal set; }
    /// <summary>The steps the damaging links are staggered by against the cosmetic ones, half the spacing: -12 for the backward sweep or 12 for the forward.</summary>
    public int InterleavedSegmentOffset { get; internal set; }
    /// <summary>The falling explosion angle: 256 steps backward or 384 forward; crossing it is the silent second joint turn.</summary>
    public NuclearWaffleSweepAngle FallingTurnThreshold { get; internal set; }
    /// <summary>The rising explosion angle: 384 steps backward or 256 forward; crossing it is the joint turn that queues the turn sound.</summary>
    public NuclearWaffleSweepAngle RisingTurnThreshold { get; internal set; }
    /// <summary>Which turn threshold the head last crossed; a change spawns two turn effect actors at the head's previous position.</summary>
    public NuclearWaffleTurnState HeadOrientationState { get; internal set; }
    /// <summary>The owning enemy's artwork and colours, captured at room load for the persistent body links and transient joint-turn effect actors.</summary>
    public SpriteGraphics Graphics { get; internal set; }

    /// <summary>The four persistent, damaging links, each an enemy projectile, in the order the sweep walks them.</summary>
    public required IReadOnlyList<NuclearWaffleLink<EnemyProjectile>> ProjectileLinks { get; init; }

    /// <summary>
    /// The three persistent, cosmetic links interleaved with the projectile links, each an effect actor; their turns are
    /// tracked apart from the projectile links'.
    /// </summary>
    public required IReadOnlyList<NuclearWaffleLink<RoomEffectActor>> SpriteLinks { get; init; }

    /// <summary>A sweep's start: the head and every link stand before both turns again.</summary>
    internal void RestartTurns()
    {
        HeadOrientationState = NuclearWaffleTurnState.BeforeTurns;
        foreach (NuclearWaffleLink<EnemyProjectile> link in ProjectileLinks)
            link.TurnState = NuclearWaffleTurnState.BeforeTurns;
        foreach (NuclearWaffleLink<RoomEffectActor> link in SpriteLinks)
            link.TurnState = NuclearWaffleTurnState.BeforeTurns;
    }
}

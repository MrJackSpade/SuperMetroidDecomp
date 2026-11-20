using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The state of the Nuclear Waffle, with its projectile and effect-actor links. The articulated body links
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
    /// The AI updates left in the wait, the one the sweep starts on included. It starts at the spawn settings' wait time and
    /// the update it runs out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>), and the update that takes it to zero
    /// starts the sweep and reloads it (#2281).
    /// </summary>
    public UpdateCountdown WaitingTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The sweep angle: its whole step is the head's angle and its fraction carries between updates. It restarts at the
    /// start angle of the sweep its spawn settings run (<see cref="NuclearWaffleSpawnSettings.Sweep"/>, #2474) with no
    /// fraction and moves monotonically toward its end angle until the last
    /// damaging link reaches it, so it stays between 140 and 500 steps, counted on past the full turn rather than wrapping;
    /// the orbit reads only its direction within the turn.
    /// </summary>
    public NuclearWaffleSweepAngle SweepAngle { get; internal set; }

    /// <summary>The head's current angle: the whole step of <see cref="SweepAngle"/>.</summary>
    public NuclearWaffleSweepAngle CurrentAngle => SweepAngle.WholeStep;

    /// <summary>The arc origin: initial population position in whole room pixels, retained as the fixed orbit center.</summary>
    public Position16<RoomPixels> Origin { get; internal set; }
    /// <summary>Which turn threshold the head last crossed; a change spawns two turn effect actors at the head's previous position.</summary>
    public NuclearWaffleTurnState HeadOrientationState { get; internal set; }
    /// <summary>The owning enemy's artwork and colours, captured at room load for the persistent body links and transient joint-turn effect actors.</summary>
    public SpriteGraphics Graphics { get; internal set; }

    /// <summary>The persistent, damaging links, each an enemy projectile, in the order the sweep walks them.</summary>
    public required IReadOnlyList<NuclearWaffleLink<EnemyProjectile>> ProjectileLinks { get; init; }

    /// <summary>
    /// The persistent, cosmetic links interleaved with the projectile links, each an effect actor; their turns are
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

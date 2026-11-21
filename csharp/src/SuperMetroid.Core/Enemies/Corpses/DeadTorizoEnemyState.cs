using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One dead Torizo's corpse state: its pose, phase and fall speed, rot schedule, its own rows as the rot has left them,
/// contact delay and the rot updates its sand heap forms by.
/// </summary>
public sealed class DeadTorizoEnemyState
{
    /// <summary>Captures the corpse's own rot schedule and its first phase.</summary>
    internal DeadTorizoEnemyState(CorpseRotSchedule rot, CorpsePhase phase)
    {
        Phase = phase;
        Rot = Ensure.NotNull(rot);
    }

    /// <summary>The corpse's typed pose and where it stands in it (#641, #1600).</summary>
    internal AnimationPlayback<DeadTorizoSegment> Playback { get; set; }

    /// <summary>The corpse's own rot schedule: the fall of each of its pixel rows.</summary>
    internal CorpseRotSchedule Rot { get; }

    /// <summary>The corpse's body rows as its rot has left them, which its body frame is drawn whole through (#1539).</summary>
    internal RottingCorpseRows Rows { get; } = new(DeadTorizoCorpseDefinitions.RowCount);
    /// <summary>
    /// The contact delay before rotting, counted once each update: its last enters rotting and the corpse never returns
    /// to the delay. Direct shot or touch triggering bypasses it.
    /// </summary>
    public UpdateCountdown PreRotDelay { get; internal set; } = UpdateCountdown.Lasting(CorpseRotScheduleDefinitions.PreRotDelayUpdates);
    /// <summary>
    /// The sand heap's forming, counted once each of the corpse's rot updates until its last line has fallen: one more line
    /// falls each <see cref="DeadTorizoCorpseDefinitions.RotUpdatesPerSandLine"/> of its age.
    /// </summary>
    public UpdateCountdown SandForming { get; } =
        UpdateCountdown.Lasting(DeadTorizoCorpseDefinitions.SandLines * DeadTorizoCorpseDefinitions.RotUpdatesPerSandLine);

    /// <summary>The corpse phase.</summary>
    internal CorpsePhase Phase { get; set; }

    /// <summary>
    /// How fast the rotting corpse sinks, downward positive: rotting increases it by
    /// <see cref="DeadTorizoCorpseDefinitions.RotGravity"/> each update (#2432).
    /// </summary>
    public SignedEightEight FallSpeed { get; internal set; }
}

using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Typed corpse state for one dead Zoomer, Ripper, or Skree.</summary>
public sealed class DeadTourianCorpseEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DeadTourianSegment> Playback { get; set; }

    /// <summary>Captures the corpse's own rot schedule, and starts its own rows and its first phase.</summary>
    internal DeadTourianCorpseEnemyState(CorpseRotSchedule rot, CorpsePhase phase)
    {
        Phase = phase;
        Rot = Ensure.NotNull(rot);
        Rows = new RottingCorpseRows(rot.Rows.Count);
    }

    /// <summary>The phase main AI runs.</summary>
    public CorpsePhase Phase { get; internal set; }
    /// <summary>The corpse's own rot schedule: the fall of each of its pixel rows, as many as its definition gives it.</summary>
    internal CorpseRotSchedule Rot { get; }
    /// <summary>The corpse's own pixel rows as its rot has left them, which its frames are drawn through (#1539).</summary>
    internal RottingCorpseRows Rows { get; }
    /// <summary>
    /// The wait before rotting, counted once each pre-rot AI update after solid Samus collision: its last switches to
    /// rotting and the corpse never returns to the delay. Touch/shot reactions enter rotting directly.
    /// </summary>
    public UpdateCountdown PreRotDelay { get; } = UpdateCountdown.Lasting(CorpseRotScheduleDefinitions.PreRotDelayUpdates);
}

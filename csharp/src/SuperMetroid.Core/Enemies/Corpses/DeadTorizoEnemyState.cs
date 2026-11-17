using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One dead Torizo's corpse state: its pose, phase and velocity, rot schedule, its own rows as the rot has left them,
/// contact delay and the rot calls its sand heap forms by.
/// </summary>
public sealed class DeadTorizoEnemyState
{
    /// <summary>Captures the corpse's own rot schedule and its first phase.</summary>
    internal DeadTorizoEnemyState(CorpseRotSchedule rot, DeadTorizoPhase phase)
    {
        Phase = phase;
        Rot = Ensure.NotNull(rot);
    }

    /// <summary>The corpse's typed pose and where it stands in it (#641, #1600).</summary>
    internal AnimationPlayback<DeadTorizoSegment> Playback { get; set; }

    /// <summary>The corpse's own rot schedule: the fall of each of its ninety-six pixel rows.</summary>
    internal CorpseRotSchedule Rot { get; }

    /// <summary>The corpse's body rows as its rot calls have left them, which its body frame is drawn whole through (#1539).</summary>
    internal RottingCorpseRows Rows { get; } = new(DeadTorizoCorpseDefinitions.RowCount);
    /// <summary>Contact-delay updates counted before rotting, 0..16: the sixteenth enters rotting and the corpse never returns to the delay. Direct shot or touch triggering bypasses it.</summary>
    public int PreRotDelayCounter { get; internal set; }
    /// <summary>Count of the corpse's rot updates since initialization, including the update that completes its final entry; its sand heap forms by it.</summary>
    public int RotUpdateCount { get; internal set; }

    /// <summary>The corpse phase.</summary>
    internal DeadTorizoPhase Phase { get; set; }

    /// <summary>Signed 1/256-pixel velocity per update: X, and Y, which rotting increases by one 256th each update.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
}

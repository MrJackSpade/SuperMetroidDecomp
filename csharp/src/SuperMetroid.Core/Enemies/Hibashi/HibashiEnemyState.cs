using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The state of one Hibashi part. Every shipped pillar uses two consecutive records: part zero owns art and timing,
/// while the nonzero part is the invisible collision body manipulated by part zero. Every value here is one the
/// cartridge keeps in the enemy's common record (its variables and population parameters), so it is emptied with
/// that record when the part is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class HibashiEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<HibashiProgram> Program { get; set; }

    /// <summary>The graphics part's behaviour, initialized to inactive; collision parts do not dispatch it, and none once a kill has emptied the record.</summary>
    public HibashiEnemyFunction? Function { get; internal set; }

    /// <summary>Countdown decremented once per inactive AI update; zero wraps to $FFFF and erupts immediately, while a nonnegative reset N waits N+1 updates.</summary>
    public WrappingCountdown InactiveTimer { get; internal set; }

    /// <summary>Animation-to-AI handshake: cleared when an eruption starts, set by the finishing cue after the hitbox is disabled, and awaited by active AI.</summary>
    public bool ActivityFinished { get; internal set; }

    /// <summary>The graphics part's room Y captured at initialization; only Y is meaningful. Activity frames place the collision part relative to it and the finishing cue restores the art owner to it.</summary>
    public Position16<RoomPixels> SpawnHeight { get; internal set; }

    /// <summary>The inactive countdown's restart value, population parameter one.</summary>
    public WrappingCountdown InactiveTimerReset { get; internal set; }

    /// <summary>Whether this is the collision part (population parameter two nonzero) rather than the graphics part.</summary>
    public bool IsCollisionPart { get; internal set; }

    /// <summary>Empties every value, when a kill empties the part's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        InactiveTimer = WrappingCountdown.Start(0);
        ActivityFinished = false;
        SpawnHeight = Position16<RoomPixels>.Zero;
        InactiveTimerReset = WrappingCountdown.Start(0);
        IsCollisionPart = false;
    }
}

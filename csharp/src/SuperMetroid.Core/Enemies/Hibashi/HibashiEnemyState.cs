using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The state of one Hibashi part. Every shipped pillar uses two consecutive records: part zero owns art and timing,
/// while the nonzero part is the invisible collision body manipulated by part zero.
/// </summary>
public sealed class HibashiEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HibashiSegment> Playback { get; set; }

    /// <summary>The graphics part's behaviour, initialized to inactive; collision parts do not dispatch it.</summary>
    public HibashiEnemyFunction? Function { get; internal set; }

    /// <summary>Countdown decremented once per inactive AI update; zero steps to -1 and erupts immediately, while a nonnegative reset N waits N+1 updates.</summary>
    public int InactiveTimer { get; internal set; }

    /// <summary>Animation-to-AI handshake: cleared when an eruption starts, set by the finishing cue after the hitbox is disabled, and awaited by active AI.</summary>
    public bool ActivityFinished { get; internal set; }

    /// <summary>The graphics part's room Y captured at initialization; only Y is meaningful. Activity frames place the collision part relative to it and the finishing cue restores the art owner to it.</summary>
    public Position16<RoomPixels> SpawnHeight { get; internal set; }

    /// <summary>The inactive countdown's restart value, population parameter one.</summary>
    public int InactiveTimerReset { get; internal set; }

    /// <summary>Whether this is the collision part (population parameter two nonzero) rather than the graphics part.</summary>
    public bool IsCollisionPart { get; internal set; }
}

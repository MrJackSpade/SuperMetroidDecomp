using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The state of a Hibashi's graphics part, which owns the pillar's art and timing. Every shipped pillar lists it right
/// before its invisible collision part (<see cref="HibashiCollisionPartState"/>), which it moves and sizes.
/// </summary>
public sealed class HibashiEnemyState
{
    /// <summary>Starts the graphics part in <paramref name="action"/>.</summary>
    internal HibashiEnemyState(HibashiAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HibashiSegment> Playback { get; set; }

    /// <summary>The graphics part's behaviour, initialized to inactive.</summary>
    public HibashiAction Action { get; internal set; }

    /// <summary>Countdown decremented once per inactive AI update; zero steps to -1 and erupts immediately, while a nonnegative reset N waits N+1 updates.</summary>
    public int InactiveTimer { get; internal set; }

    /// <summary>Animation-to-AI handshake: cleared when an eruption starts, set by the finishing cue after the hitbox is disabled, and awaited by active AI.</summary>
    public bool ActivityFinished { get; internal set; }

    /// <summary>The graphics part's room Y captured at initialization. Activity frames place the collision part relative to it and the finishing cue restores the art owner to it.</summary>
    public int SpawnY { get; internal set; }

    /// <summary>The inactive countdown's restart value, its spawn inactive time.</summary>
    public int InactiveTimerReset { get; internal set; }

}

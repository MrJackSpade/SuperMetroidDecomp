using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The state of one Hibashi part. Every shipped pillar lists two consecutive parts: the visible flame owns the art and
/// timing, and the collision part after it is the invisible body the flame moves.
/// </summary>
public sealed class HibashiEnemyState
{
    /// <summary>Starts the graphics part in <paramref name="action"/>.</summary>
    internal HibashiEnemyState(HibashiAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HibashiSegment> Playback { get; set; }

    /// <summary>The flame's behaviour, initialized to inactive.</summary>
    public HibashiAction Action { get; internal set; }

    /// <summary>
    /// The inactive updates left before the next eruption, the one it erupts on included (#2340): a new flame starts with
    /// none left and erupts on its first update, and each eruption restarts the wait at its placement's inactive time and
    /// the update it runs out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>, #2340).
    /// </summary>
    public int InactiveTimer { get; internal set; }

    /// <summary>The flame's room Y captured at initialization. Activity frames place the collision part relative to it and the finishing cue restores the art owner to it.</summary>
    public int SpawnY { get; internal set; }
}

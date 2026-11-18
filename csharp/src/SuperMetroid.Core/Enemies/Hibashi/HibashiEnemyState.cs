using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The state of a Hibashi's visible flame, which holds the pillar's pictures and timing and moves and sizes the invisible
/// collision part listed right after it (<see cref="HibashiCollisionPartState"/>).
/// </summary>
public sealed class HibashiEnemyState
{
    /// <summary>Starts the flame in <paramref name="action"/>.</summary>
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

    /// <summary>The flame's room Y where it was placed. The eruption's steps raise the collision part from it and its last cue returns the flame to it.</summary>
    public int SpawnY { get; internal set; }
}

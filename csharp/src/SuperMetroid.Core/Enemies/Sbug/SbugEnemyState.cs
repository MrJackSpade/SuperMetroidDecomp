using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// The state of an Sbug. The names follow the behaviour the Sbug's movement establishes. It reads its speed, spawn heading,
/// activation distance and activated behaviour from its spawn settings where it uses them, and the straight legs they give
/// it (<see cref="SbugLeg"/>) with them (#2450).
/// </summary>
public sealed class SbugEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal SbugSpawnSettings Settings { get; }

    /// <summary>Creates the state of an Sbug starting in <paramref name="action"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal SbugEnemyState(SbugAction action, SbugSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Action = action;
    }

    /// <summary>The current AI behaviour.</summary>
    public SbugAction Action { get; internal set; }

    /// <summary>The updates left in a forward-then-wait leg, the one it ends on included (<see cref="SbugMovementDefinitions.LegDuration"/>, #2281).</summary>
    public UpdateCountdown MovementTimer { get; internal set; } = UpdateCountdown.Ended();



    /// <summary>
    /// The animation the enemy plays and where it stands in it (#1600): none before it selects its first direction, then
    /// the loop of the facing it last selected, which selecting that facing again leaves playing (#2145).
    /// </summary>
    internal AnimationPlayback<SbugSegment> Playback { get; set; }
}

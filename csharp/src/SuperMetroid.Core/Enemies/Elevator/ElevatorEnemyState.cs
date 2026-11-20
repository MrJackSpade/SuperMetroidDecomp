using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The state of the elevator enemy.</summary>
public sealed class ElevatorEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal ElevatorSpawnSettings Settings { get; }

    /// <summary>Creates the state of an enemy whose entry told its species <paramref name="settings"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal ElevatorEnemyState(ElevatorSpawnSettings settings) => Settings = Ensure.NotNull(settings);

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ElevatorSegment> Playback { get; set; }

    /// <summary>
    /// The room-side resting room Y, captured before an arriving actor is moved to its arrival height. Which way the
    /// elevator departs, which input starts it and which way an arrival returns to rest are its spawn settings' direction,
    /// which it reads where it uses it (#2476).
    /// </summary>
    public int RestingY { get; internal set; }
}

using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The state of one Spark.</summary>
public sealed class SparkEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal SparkSpawnSettings Settings { get; }

    /// <summary>Starts the state in <paramref name="action"/>, playing <paramref name="playback"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal SparkEnemyState(SparkAction action, AnimationPlayback<SparkSegment> playback, SparkSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Action = action;
        Playback = playback;
    }

    /// <summary>The animation the Spark plays and where it stands in it (#1600), which its initialization starts.</summary>
    internal AnimationPlayback<SparkSegment> Playback { get; set; }

    /// <summary>The always-active, intermittent or falling-spark behaviour.</summary>
    public SparkAction Action { get; internal set; }


    /// <summary>
    /// The wait for the Spark's next action, lasting its base time: the update it ends on expires it. Only an always-active
    /// Spark has a base time of zero, and it never counts its wait (#1846).
    /// </summary>
    public UpdateCountdown ActionTimer { get; internal set; } = UpdateCountdown.Ended();
}

using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The state of one Spark.</summary>
public sealed class SparkEnemyState
{
    /// <summary>The animation the Spark plays and where it stands in it (#1600); none until the Spark is initialized.</summary>
    internal AnimationPlayback<SparkSegment>? Playback { get; set; }

    /// <summary>The always-active, intermittent or falling-spark behaviour.</summary>
    public SparkAction? Action { get; internal set; }

    /// <summary>The updates the Spark waits between actions when it does not randomize them, from its spawn settings.</summary>
    public int BaseActionTime { get; internal set; }


    /// <summary>
    /// The countdown to the Spark's next action, from its base time: the update whose step would reach zero expires it.
    /// Only an always-active Spark has a base time of zero, and it never counts down (#1846).
    /// </summary>
    public int ActionTimer { get; internal set; }
}

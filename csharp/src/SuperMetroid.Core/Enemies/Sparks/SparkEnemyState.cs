using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The state of one Spark.</summary>
public sealed class SparkEnemyState
{
    /// <summary>The animation the Spark plays and where it stands in it (#1600); none until the Spark is initialized.</summary>
    internal AnimationPlayback<SparkSegment>? Playback { get; set; }

    /// <summary>The always-active, intermittent or falling-spark behaviour.</summary>
    public SparkAction? Action { get; internal set; }

    /// <summary>The updates the Spark waits between functions when it does not randomize them, from its spawn settings.</summary>
    public int BaseActionTime { get; internal set; }


    /// <summary>The countdown, decremented and then tested for zero as the game does: reaching zero expires it, and one that starts from zero wraps instead.</summary>
    public int ActionTimer { get; internal set; }
}

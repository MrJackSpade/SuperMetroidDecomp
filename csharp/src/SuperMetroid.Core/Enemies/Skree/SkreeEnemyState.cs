using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The state of a Skree.</summary>
public sealed class SkreeEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal SkreeEnemyState(SkreeAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SkreeMetareeSegment> Playback { get; set; }

    /// <summary>
    /// The updates left of the burrow, the one the Skree is gone on included: started at
    /// <see cref="SkreeMotionDefinitions.BurrowDuration"/> as the dive meets the floor (#2454).
    /// </summary>
    public UpdateCountdown BurrowTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The current action.</summary>
    public SkreeAction Action { get; internal set; }
}

using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The state of a Skree.</summary>
public sealed class SkreeEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal SkreeEnemyState(SkreeAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SkreeMetareeSegment> Playback { get; set; }

    /// <summary>Reloaded to 21 while diving and counted down while burrowing.</summary>
    public int BurrowTimer { get; internal set; }

    /// <summary>The current action.</summary>
    public SkreeAction Action { get; internal set; }
}

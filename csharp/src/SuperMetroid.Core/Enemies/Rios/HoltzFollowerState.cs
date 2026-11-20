using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of a Holtz's flame follower, the half its room lists right after the parent (#2063): only its
/// looping flames. It reads its placement and visibility from its parent.
/// </summary>
public sealed class HoltzFollowerState
{
    /// <summary>Starts the follower's state with the parent it found when it was placed.</summary>
    /// <param name="parent">The parent's state.</param>
    internal HoltzFollowerState(HoltzEnemyState parent) => Parent = Ensure.NotNull(parent);

    /// <summary>The parent the follower found when it was placed, the Holtz its room lists right before it (#2554).</summary>
    internal HoltzEnemyState Parent { get; }

    /// <summary>The animation the follower plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HoltzSegment> Playback { get; set; }
}

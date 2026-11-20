using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of a Holtz's flame follower, the half its room lists right after the parent (#2063): only its
/// looping flames. It reads its placement and visibility from its parent.
/// </summary>
public sealed class HoltzFollowerState
{
    /// <summary>The animation the follower plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HoltzSegment> Playback { get; set; }

    /// <summary>The state of the parent the follower found as it was placed, which it holds from its setup (#2554).</summary>
    internal required HoltzEnemyState Parent { get; init; }

    /// <summary>
    /// The place of the parent the follower found as it was placed, the enemy its room lists directly before it, whatever
    /// that place holds now.
    /// </summary>
    internal required RoomEnemyPlace ParentPlace { get; init; }
}

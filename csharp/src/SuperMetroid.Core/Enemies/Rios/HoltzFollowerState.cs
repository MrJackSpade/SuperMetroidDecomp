using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of a Holtz's flame follower, the half its room lists right after the parent (#2063): only its
/// looping flames. It reads its placement and visibility from its parent.
/// </summary>
public sealed partial class HoltzFollowerState
{
    /// <summary>Creates the follower's state, following the parent it found as it was placed.</summary>
    /// <param name="parent">The parent's state.</param>
    /// <param name="parentPlace">The parent's place.</param>
    internal HoltzFollowerState(HoltzEnemyState parent, RoomEnemyPlace parentPlace)
    {
        Parent = parent;
        ParentPlace = Ensure.NotNull(parentPlace);
    }

    /// <summary>The animation the follower plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HoltzSegment> Playback { get; set; }

    /// <summary>The state of the parent the follower found as it was placed, which it holds from its setup (#2554).</summary>
    internal HoltzEnemyState Parent { get; }

    /// <summary>
    /// The place of the parent the follower found as it was placed, the enemy its room lists directly before it, whatever
    /// that place holds now.
    /// </summary>
    internal RoomEnemyPlace ParentPlace { get; }
}

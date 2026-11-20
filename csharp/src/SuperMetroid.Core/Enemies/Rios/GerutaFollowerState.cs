using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of a Geruta's flame follower, the half its room lists right after the parent (#2063): only its
/// animation. It reads its placement, visibility and flames from its parent.
/// </summary>
public sealed class GerutaFollowerState
{
    /// <summary>The animation the follower plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<GerutaSegment> Playback { get; set; }

    /// <summary>
    /// The parent the follower found when it was placed, the Rio its room lists right before it (#2554): its state as it
    /// moves on, and as a deletion without a kill left it.
    /// </summary>
    internal required GerutaEnemyState Parent { get; init; }
}

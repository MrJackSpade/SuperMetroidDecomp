using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of a Lower Norfair Rio's flame follower, the half its room lists right after the parent (#2063): only its
/// looping flames. It reads its placement and visibility from its parent.
/// </summary>
public sealed class LowerNorfairRioFollowerState
{
    /// <summary>The animation the follower plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<LowerNorfairRioSegment> Playback { get; set; }
}

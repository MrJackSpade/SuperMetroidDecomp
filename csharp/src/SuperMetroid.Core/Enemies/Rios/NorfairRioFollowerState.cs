using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of a Norfair Rio's flame follower, the half its room lists right after the parent (#2063): only its
/// animation. It reads its placement, visibility and flames from its parent.
/// </summary>
public sealed class NorfairRioFollowerState
{
    /// <summary>The animation the follower plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<NorfairRioSegment> Playback { get; set; }

    /// <summary>The last animation started, which starting it again does not restart.</summary>
    internal NorfairRioSegment InstalledAnimation { get; set; }
}

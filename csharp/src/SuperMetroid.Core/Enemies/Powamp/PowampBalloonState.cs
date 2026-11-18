using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The state of a Powamp's balloon (#1845), the half its body moves: where it spawned and its inflate or sink animation
/// with the pose it stands at. How far a grapple lifts the pair is its spawn settings', read as the body rises (#2439).
/// </summary>
public sealed class PowampBalloonState
{
    /// <summary>The balloon's inflate or sink animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PowampSegment> Playback { get; set; }

    /// <summary>Where the balloon's inflate or sink sequence stands, recorded as the animation shows each pose.</summary>
    public PowampBalloonInflation Inflation { get; internal set; }

    /// <summary>The balloon's original room position: X is both halves' horizontal wiggle center and Y feeds the body's resting and rise-target calculations.</summary>
    public Position16<RoomPixels> Spawn { get; internal set; }

    /// <summary>Whether Samus's touch and shots pass through it: from spawn for a balloon with a nonzero grapple travel distance.</summary>
    public bool RefusesContact { get; internal set; }
}

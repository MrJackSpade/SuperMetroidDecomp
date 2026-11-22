using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What every Magdollite part keeps: its animation and where it started. The head, the rising body and the tracking
/// overlay each keep the rest of their own state (<see cref="MagdolliteHeadState"/>, <see cref="MagdolliteBodyState"/>,
/// <see cref="MagdolliteOverlayState"/>).
/// </summary>
public abstract partial class MagdollitePartState
{
    /// <summary>Creates the state for one Magdollite part, which its own state type names (#2474).</summary>
    private protected MagdollitePartState()
    {
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MagdolliteSegment> Playback { get; set; }

    /// <summary>
    /// The part's whole-pixel spawn room Y, captured before its setup moves it. The head and arm return to it, and
    /// the head's sink and surface cues are measured from it.
    /// </summary>
    public int OriginY { get; internal set; }
}

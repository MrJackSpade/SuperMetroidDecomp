using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What every Magdollite part keeps: its animation and where it started. The head, the rising body and the tracking
/// overlay each keep the rest of their own state (<see cref="MagdolliteHeadState"/>, <see cref="MagdolliteBodyState"/>,
/// <see cref="MagdolliteOverlayState"/>).
/// </summary>
public abstract class MagdollitePartState
{
    /// <summary>Creates the state for one Magdollite part.</summary>
    private protected MagdollitePartState(MagdollitePart part) => Part = part;

    /// <summary>The head, arm, or hand role its spawn names.</summary>
    public MagdollitePart Part { get; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MagdolliteSegment> Playback { get; set; }

    /// <summary>
    /// Whether an animation is active, set and cleared by the head's and the overlay's animation cues to gate the head's
    /// transitions and the end of the overlay's throw.
    /// </summary>
    public bool AnimationBusy { get; internal set; }

    /// <summary>
    /// The part's whole-pixel spawn room Y, captured before its initializer moves it. The head and arm return to it, and
    /// the head's sink and surface cues are measured from it.
    /// </summary>
    public int OriginY { get; internal set; }
}

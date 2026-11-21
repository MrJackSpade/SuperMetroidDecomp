using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// A Magdollite's tracking overlay's state, the hand that rides the arm's cap and throws the lava: what it is doing, its
/// throw and the cooldown before the head attacks again.
/// </summary>
public sealed class MagdolliteOverlayState : MagdollitePartState
{
    /// <summary>Creates the overlay's state, starting in <paramref name="action"/>.</summary>
    internal MagdolliteOverlayState(MagdolliteOverlayAction action)
    {
        Action = action;
    }

    /// <summary>The animation the overlay plays and where it stands in it (#1600, #2554).</summary>
    internal AnimationPlayback<MagdolliteOverlaySegment> Playback { get; set; }

    /// <summary>What the overlay is doing.</summary>
    public MagdolliteOverlayAction Action { get; internal set; }

    /// <summary>
    /// The overlay updates before the head may attack again, the one after which it may included, counted each overlay
    /// update: the throw animation restarts it at <see cref="MagdolliteMotionDefinitions.AttackCooldown"/>, and the head,
    /// which polls it, may attack once it is over, restarting it at <see cref="MagdolliteMotionDefinitions.AttackPollWait"/>
    /// as it does.
    /// </summary>
    public UpdateCountdown AttackCooldown { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The way the hand throws its lava: toward Samus's side at the apex (#2338).</summary>
    public HorizontalDirection ThrowDirection { get; internal set; }

    /// <summary>Whole-pixel room position captured at the apex and used as the base for hand-animation shifts and restoration.</summary>
    public Position16<RoomPixels> ThrowOrigin { get; internal set; }

    /// <summary>The throw's finishing cue (#2439): the hand has thrown, and its next update starts the arm's descent.</summary>
    internal void FinishThrow() => Action = MagdolliteOverlayAction.Thrown;
}

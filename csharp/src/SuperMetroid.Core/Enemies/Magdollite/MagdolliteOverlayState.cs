namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// A Magdollite's tracking overlay's state, the hand that rides the arm's cap and throws the lava: what it is doing, its
/// throw and the cooldown before the head attacks again.
/// </summary>
public sealed class MagdolliteOverlayState : MagdollitePartState
{
    /// <summary>Creates the overlay's state, starting in <paramref name="action"/>.</summary>
    internal MagdolliteOverlayState(MagdolliteOverlayAction action)
        : base(MagdollitePart.TrackingOverlay)
    {
        Action = action;
    }

    /// <summary>What the overlay is doing.</summary>
    public MagdolliteOverlayAction Action { get; internal set; }

    /// <summary>
    /// The cooldown before the head may attack again, counted down each overlay update: the throw animation restarts it
    /// at 256, and the head, which polls it, attacks once it has run past zero.
    /// </summary>
    public WrappingCountdown AttackTimer { get; internal set; }

    /// <summary>The way the hand throws its lava: toward Samus's side at the apex (#2338).</summary>
    public HorizontalDirection ThrowDirection { get; internal set; }

    /// <summary>Whole-pixel room position captured at the apex and used as the base for hand-animation shifts and restoration.</summary>
    public Position16<RoomPixels> ThrowOrigin { get; internal set; }
}

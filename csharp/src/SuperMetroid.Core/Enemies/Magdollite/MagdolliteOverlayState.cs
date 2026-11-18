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

    /// <summary>What the overlay is doing.</summary>
    public MagdolliteOverlayAction Action { get; internal set; }

    /// <summary>
    /// The overlay updates before the head may attack again, the one after which it may included, counted down each
    /// overlay update to none: the throw animation restarts it at
    /// <see cref="MagdolliteMotionDefinitions.AttackCooldown"/>, and the head, which polls it, may attack once none are
    /// left, restarting it at <see cref="MagdolliteMotionDefinitions.AttackPollWait"/> as it does.
    /// </summary>
    public int AttackCooldown
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>The way the hand throws its lava: toward Samus's side at the apex (#2338).</summary>
    public HorizontalDirection ThrowDirection { get; internal set; }

    /// <summary>Whole-pixel room position captured at the apex and used as the base for hand-animation shifts and restoration.</summary>
    public Position16<RoomPixels> ThrowOrigin { get; internal set; }

    /// <summary>The throw's finishing cue (#2439): the hand has thrown, and its next update starts the arm's descent.</summary>
    /// <exception cref="InvalidOperationException">The hand is not waiting for its throw.</exception>
    internal void FinishThrow() => Action = Action == MagdolliteOverlayAction.WaitingForThrowAnimation
        ? MagdolliteOverlayAction.Thrown
        : throw new InvalidOperationException($"A Magdollite's hand finished an animation while it was {Action}.");
}

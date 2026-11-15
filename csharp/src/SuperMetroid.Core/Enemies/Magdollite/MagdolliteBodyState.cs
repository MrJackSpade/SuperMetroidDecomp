namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's rising body's state, the arm that grows out of the lava: what it is doing and how far it has grown.</summary>
public sealed class MagdolliteBodyState : MagdollitePartState
{
    /// <summary>Creates the body's state.</summary>
    internal MagdolliteBodyState()
        : base(MagdollitePart.RisingBody)
    {
    }

    /// <summary>What the arm is doing.</summary>
    public MagdolliteBodyAction? Action { get; internal set; }

    /// <summary>
    /// The arm's rise phase, 0 through 8, selecting its pillar stack, threshold and overlay offset from the nine
    /// <see cref="MagdolliteRise.Phase"/> phases. It steps by one as the arm rises past each phase's threshold and
    /// back as it sinks.
    /// </summary>
    public int BodyPhase { get; internal set; }

    /// <summary>
    /// The arm's whole-pixel displacement from where it started rising; only Y is meaningful, negative while the arm is
    /// above that point.
    /// </summary>
    public Offset16<RoomPixels> RiseDisplacement { get; internal set; }

    /// <summary>Whether the arm has finished growing, set by the arm at maximum or Samus-relative height and cleared by the overlay when shrinking begins.</summary>
    public bool BodyReachedApex { get; internal set; }

    /// <summary>Whether the arm has not emerged: true holds the arm dormant; the head clears it to start growth and landing sets it again.</summary>
    public bool BodyWakeRequested { get; internal set; }
}

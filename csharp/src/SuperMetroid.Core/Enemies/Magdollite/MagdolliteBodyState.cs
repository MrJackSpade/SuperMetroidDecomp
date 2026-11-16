namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's rising body's state, the arm that grows out of the lava: what it is doing and how far it has grown.</summary>
public sealed class MagdolliteBodyState : MagdollitePartState
{
    /// <summary>Creates the body's state, starting in <paramref name="action"/>.</summary>
    internal MagdolliteBodyState(MagdolliteBodyAction action)
        : base(MagdollitePart.RisingBody)
    {
        Action = action;
    }

    /// <summary>What the arm is doing.</summary>
    public MagdolliteBodyAction Action { get; internal set; }

    /// <summary>
    /// The arm's stage of its rise, which selects its pillar stack, threshold and overlay offset. It steps up as the arm
    /// rises past each stage's threshold and back down as it sinks.
    /// </summary>
    public MagdolliteRiseStage BodyPhase { get; internal set; }

    /// <summary>
    /// The arm's whole-pixel vertical displacement from where it started rising, negative while the arm is above that
    /// point.
    /// </summary>
    public int RiseDisplacementY { get; internal set; }

    /// <summary>Whether the arm has finished growing, set by the arm at maximum or Samus-relative height and cleared by the overlay when shrinking begins.</summary>
    public bool BodyReachedApex { get; internal set; }

    /// <summary>Whether the arm has not emerged: true holds the arm dormant; the head clears it to start growth and landing sets it again.</summary>
    public bool BodyWakeRequested { get; internal set; }
}

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite head's state: what it is doing, whether it waits for its arm to land, and the speed its arm moves at.</summary>
public sealed class MagdolliteHeadState : MagdollitePartState
{
    /// <summary>Creates the head's state, starting in <paramref name="action"/>.</summary>
    internal MagdolliteHeadState(MagdolliteHeadAction action)
        : base(MagdollitePart.Head)
    {
        Action = action;
    }

    /// <summary>What the head is doing.</summary>
    public MagdolliteHeadAction Action { get; internal set; }

    /// <summary>Whether the head is not yet ready to emerge: set by the head after submerging and cleared by the arm on landing so the head may emerge.</summary>
    public bool HeadWaitsForBodyLanding { get; internal set; }

    /// <summary>
    /// The head's speed in pixels per update, from its spawn settings, at which its arm rises and sinks: the arm reads
    /// the head's, adding it as it sinks and negating it as it rises.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }
}

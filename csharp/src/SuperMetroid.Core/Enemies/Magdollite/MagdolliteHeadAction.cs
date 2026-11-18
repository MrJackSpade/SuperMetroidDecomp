namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a Magdollite's head is doing. Its values are arbitrary.
/// </summary>
public enum MagdolliteHeadAction
{
    /// <summary>Faces Samus and submerges when the overlay's cooldown has expired and Samus is within the configured horizontal range.</summary>
    Waiting,
    /// <summary>Waits for the head's submerge animation to finish.</summary>
    WaitingForAttackAnimation,
    /// <summary>The submerge animation has finished, as its cue says (#2439): the next update releases the arm to grow.</summary>
    Submerged,
    /// <summary>Waits for the arm to land.</summary>
    WaitingForBodyLanding,
    /// <summary>The arm has landed (#2439): the next update selects the facing-dependent head emergence animation.</summary>
    ArmLanded,
    /// <summary>Waits for emergence to finish.</summary>
    WaitingForReturnAnimation,
    /// <summary>The emergence has finished, as its cue says (#2439): the next update returns the head to idle.</summary>
    Surfaced,
}

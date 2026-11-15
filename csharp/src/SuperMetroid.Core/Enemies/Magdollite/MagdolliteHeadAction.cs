namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a Magdollite's head is doing. Its values are arbitrary.
/// </summary>
public enum MagdolliteHeadAction
{
    /// <summary>Faces Samus and submerges when the overlay's cooldown has expired and Samus is within the configured horizontal range.</summary>
    Waiting,
    /// <summary>Waits for the head's submerge animation to finish, then releases the arm to grow.</summary>
    WaitingForAttackAnimation,
    /// <summary>Waits for the arm to land, then selects the facing-dependent head emergence animation.</summary>
    WaitingForBodyLanding,
    /// <summary>Waits for emergence to finish before returning the head to idle.</summary>
    WaitingForReturnAnimation,
}

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// The enemy definition's grapple reaction. Every retail
/// enemy selects one of the seven shared reactions except the elevator, whose
/// main AI doubles as its grapple AI.
/// </summary>
public enum GrappleAiRoutine
{
    /// <summary>No definition: an empty enemy slot's default.</summary>
    None,

    /// <summary>The grapple passes through without interacting.</summary>
    NoInteraction,

    /// <summary>Samus latches onto the enemy.</summary>
    Attach,

    /// <summary>The grapple kills the enemy.</summary>
    Kill,

    /// <summary>The enemy cancels the grapple beam.</summary>
    Cancel,

    /// <summary>Samus latches on without the enemy becoming invincible.</summary>
    AttachWithoutInvincibility,

    /// <summary>Samus latches on and the enemy is paralyzed.</summary>
    AttachAndParalyze,

    /// <summary>The enemy hurts Samus through the grapple.</summary>
    HurtSamus,

    /// <summary>The elevator's main AI, which grapple also uses for frozen enemies.</summary>
    ElevatorMainAi,
}

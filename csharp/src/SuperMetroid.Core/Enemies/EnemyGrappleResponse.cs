namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What an enemy does when the grapple beam reaches it. Every retail enemy names one of the seven shared responses except
/// the elevator, whose own behaviour carries on.
/// </summary>
public enum EnemyGrappleResponse
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

    /// <summary>The species' own behaviour carries on, as only the elevator's definition names; the grapple has no response to it.</summary>
    SpeciesOwn,
}

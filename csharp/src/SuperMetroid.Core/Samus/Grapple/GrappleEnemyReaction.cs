namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How an enemy the grapple beam's tip touches reacts to it.</summary>
public enum GrappleEnemyReaction
{
    /// <summary>The enemy ignores the grapple beam.</summary>
    None,
    /// <summary>The beam latches on and the enemy becomes invincible while Samus hangs from it.</summary>
    Attach,
    /// <summary>The beam kills the enemy.</summary>
    Kill,
    /// <summary>The enemy stops the extending beam.</summary>
    Cancel,
    /// <summary>The beam latches on without making the enemy invincible.</summary>
    AttachWithoutInvincibility,
    /// <summary>The beam latches on and paralyzes the enemy.</summary>
    AttachAndParalyze,
    /// <summary>Touching the enemy with the beam hurts Samus.</summary>
    HurtSamus,
    /// <summary>
    /// The enemy has no reaction to the grapple beam (#2283, #2558): the elevator, whose own behaviour carries on. The beam
    /// never meets it, and reaching it fails.
    /// </summary>
    Unreactive,
}

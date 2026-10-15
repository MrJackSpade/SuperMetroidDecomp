namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// The seven indices returned by <c>EnemyGrappleBeamCollisionDetection</c> at $A0:9E9A.
/// Values intentionally match bank-$9B's jump table rather than introducing host ordering.
/// </summary>
public enum GrappleEnemyReaction : ushort
{
    /// <summary>The enemy does not react to the grapple endpoint.</summary>
    None = 0,
    /// <summary>The beam attaches and grants the ordinary connection behavior.</summary>
    Attach = 1,
    /// <summary>The grapple endpoint kills the enemy.</summary>
    Kill = 2,
    /// <summary>The enemy cancels the extending beam.</summary>
    Cancel = 3,
    /// <summary>The beam attaches without granting enemy invincibility.</summary>
    AttachWithoutInvincibility = 4,
    /// <summary>The beam attaches and paralyzes the enemy.</summary>
    AttachAndParalyze = 5,
    /// <summary>The grapple interaction damages Samus.</summary>
    HurtSamus = 6,
}

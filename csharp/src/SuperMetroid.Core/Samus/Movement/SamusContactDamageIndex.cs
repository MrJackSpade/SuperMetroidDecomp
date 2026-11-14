namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Which attack Samus's body is, if any, when an enemy touches her: enemy touch handling decides from it whether the
/// enemy hurts her or she hurts the enemy.
/// </summary>
public enum SamusContactDamageIndex
{
    /// <summary>Her body is no attack, so an enemy that touches her hurts her; a hurt or knockback returns her to it.</summary>
    None,

    /// <summary>The Speed Booster's charge, once her run reaches its fourth boost stage.</summary>
    SpeedBooster,

    /// <summary>A shinespark in flight.</summary>
    Shinespark,

    /// <summary>The Screw Attack's spin.</summary>
    ScrewAttack,

    /// <summary>A fully charged beam held through an ordinary spin jump: the pseudo Screw Attack.</summary>
    PseudoScrew,
}

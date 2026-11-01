namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's contact-damage index. Movement stores only these five
/// values; enemy touch handling selects Samus's attacking body (or ordinary touch damage) from it.
/// </summary>
public enum SamusContactDamageIndex
{
    /// <summary>Zero: no attacking body, so enemy contact damages Samus (cleared and by hurt/knockback).</summary>
    None = 0,

    /// <summary>One: Speed Booster contact attack, published at boost stage four.</summary>
    SpeedBooster = 1,

    /// <summary>Two: shinespark contact attack.</summary>
    Shinespark = 2,

    /// <summary>Three: Screw Attack spin.</summary>
    ScrewAttack = 3,

    /// <summary>Four: pseudo Screw Attack, a fully charged ordinary spin.</summary>
    PseudoScrew = 4,
}

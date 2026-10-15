namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's contact-damage index at WRAM <c>$0A6E</c>. Bank <c>$90</c> stores only these five
/// values; enemy touch handling selects Samus's attacking body (or ordinary touch damage) from it.
/// </summary>
public enum SamusContactDamageIndex : ushort
{
    /// <summary>Zero: no attacking body, so enemy contact damages Samus (cleared at <c>$90:E725</c> and by hurt/knockback).</summary>
    None = 0,

    /// <summary>One: Speed Booster contact attack, published at boost stage four (<c>$90:981E</c>).</summary>
    SpeedBooster = 1,

    /// <summary>Two: shinespark contact attack (<c>$90:D0AB</c>, <c>$90:D0D7</c>, <c>$90:D106</c>).</summary>
    Shinespark = 2,

    /// <summary>Three: Screw Attack spin (<c>$90:A509</c>, <c>$90:A754</c>).</summary>
    ScrewAttack = 3,

    /// <summary>Four: pseudo Screw Attack, a fully charged ordinary spin (<c>$90:A501</c>, <c>$90:A74C</c>).</summary>
    PseudoScrew = 4,
}

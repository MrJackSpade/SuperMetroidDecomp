namespace SuperMetroid.Core.Progression;

/// <summary>
/// Samus's independent items, one flag each. The values are arbitrary and nothing reads them: the save slot equipment
/// word is the import's SamusEquipmentWordCodec's, and the animation item operand is
/// the Samus animation's item operand encoding's. Members keep the word's bit order, so enumerating them lists
/// the items in that order.
/// </summary>
[Flags]
public enum SamusEquipmentFlags
{
    /// <summary>No suit, mobility, explosive, or exploration item bits are enabled.</summary>
    None = 0,

    /// <summary>Enables the Varia Suit's heat protection and first-tier damage reduction.</summary>
    VariaSuit = 1 << 0,

    /// <summary>Allows Samus to jump while in morph-ball form.</summary>
    SpringBall = 1 << 1,

    /// <summary>Allows Samus to enter morph-ball form.</summary>
    MorphBall = 1 << 2,

    /// <summary>Turns spin-jump contact into the Screw Attack's damaging collision.</summary>
    ScrewAttack = 1 << 3,

    /// <summary>
    /// The item slot no item occupies. The Golden Torizo item code and one attract demonstration set it, so it is
    /// carried with the other items; nothing in play grants an ability for it.
    /// </summary>
    UnassignedItem = 1 << 4,

    /// <summary>Enables Gravity Suit movement protection and the cartridge's strongest damage reduction.</summary>
    GravitySuit = 1 << 5,

    /// <summary>Applies the Hi-Jump Boots' increased jump acceleration.</summary>
    HiJumpBoots = 1 << 6,

    /// <summary>Allows repeated aerial jumps while Samus remains in a spin-jump state.</summary>
    SpaceJump = 1 << 7,

    /// <summary>Allows morph-ball bomb placement.</summary>
    Bombs = 1 << 8,

    /// <summary>Enables speed-boost charge accumulation while running.</summary>
    SpeedBooster = 1 << 9,

    /// <summary>Makes the Grapple Beam selectable in the HUD.</summary>
    GrappleBeam = 1 << 10,

    /// <summary>Makes the X-ray Scope selectable in the HUD.</summary>
    XrayScope = 1 << 11,
}

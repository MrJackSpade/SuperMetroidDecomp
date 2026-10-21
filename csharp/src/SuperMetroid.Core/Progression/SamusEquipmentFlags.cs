namespace SuperMetroid.Core.Progression;

/// <summary>
/// Independent item bits stored in Samus's native equipped-items word at WRAM
/// <c>$09A2</c>. The missing bit values are unused by the translated retail data and are
/// deliberately not assigned speculative names.
/// </summary>
[Flags]
public enum SamusEquipmentFlags : ushort
{
    /// <summary>No suit, mobility, explosive, or exploration item bits are enabled.</summary>
    None = 0,

    /// <summary>Enables the Varia Suit's heat protection and first-tier damage reduction.</summary>
    VariaSuit = 0x0001,

    /// <summary>Allows Samus to jump while in morph-ball form.</summary>
    SpringBall = 0x0002,

    /// <summary>Allows Samus to enter morph-ball form.</summary>
    MorphBall = 0x0004,

    /// <summary>Turns spin-jump contact into the Screw Attack's damaging collision.</summary>
    ScrewAttack = 0x0008,

    /// <summary>Enables Gravity Suit movement protection and the cartridge's strongest damage reduction.</summary>
    GravitySuit = 0x0020,

    /// <summary>Applies the Hi-Jump Boots' increased jump acceleration.</summary>
    HiJumpBoots = 0x0100,

    /// <summary>Allows repeated aerial jumps while Samus remains in a spin-jump state.</summary>
    SpaceJump = 0x0200,

    /// <summary>Allows morph-ball bomb placement.</summary>
    Bombs = 0x1000,

    /// <summary>Enables speed-boost charge accumulation while running.</summary>
    SpeedBooster = 0x2000,

    /// <summary>Makes the Grapple Beam selectable in the HUD.</summary>
    GrappleBeam = 0x4000,

    /// <summary>Makes the X-ray Scope selectable in the HUD.</summary>
    XrayScope = 0x8000,
}

namespace SuperMetroid.Core.Progression;

/// <summary>
/// Samus's independent items, one bit each, in the bit order of the equipment word
/// (<see cref="SamusEquipmentWordCodec"/>). The missing bit values are unused by the retail
/// data and are deliberately not assigned speculative names.
/// </summary>
[Flags]
public enum SamusEquipmentFlags : ushort
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

    /// <summary>Enables Gravity Suit movement protection and the cartridge's strongest damage reduction.</summary>
    GravitySuit = 1 << 5,

    /// <summary>Applies the Hi-Jump Boots' increased jump acceleration.</summary>
    HiJumpBoots = 1 << 8,

    /// <summary>Allows repeated aerial jumps while Samus remains in a spin-jump state.</summary>
    SpaceJump = 1 << 9,

    /// <summary>Allows morph-ball bomb placement.</summary>
    Bombs = 1 << 12,

    /// <summary>Enables speed-boost charge accumulation while running.</summary>
    SpeedBooster = 1 << 13,

    /// <summary>Makes the Grapple Beam selectable in the HUD.</summary>
    GrappleBeam = 1 << 14,

    /// <summary>Makes the X-ray Scope selectable in the HUD.</summary>
    XrayScope = 1 << 15,
}

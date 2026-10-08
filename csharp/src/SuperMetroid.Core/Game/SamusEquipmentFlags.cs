namespace SuperMetroid.Core.Game;

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

/// <summary>
/// Independent beam-equipment bits stored at WRAM <c>$09A6</c>. Low-nibble beam bits may
/// be combined; Charge occupies bit twelve just as it does in the cartridge word.
/// </summary>
[Flags]
public enum SamusBeamFlags : ushort
{
    /// <summary>The unmodified Power Beam, with no optional beam components enabled.</summary>
    None = 0,

    /// <summary>Adds the Wave Beam's terrain-passing behavior and waveform presentation.</summary>
    Wave = 0x0001,

    /// <summary>Adds the Ice Beam's enemy-freezing behavior.</summary>
    Ice = 0x0002,

    /// <summary>Adds the Spazer's widened three-part projectile; mutually exclusive with Plasma in retail equipment UI.</summary>
    Spazer = 0x0004,

    /// <summary>Adds the Plasma Beam's penetrating high-damage projectile; mutually exclusive with Spazer in retail equipment UI.</summary>
    Plasma = 0x0008,

    /// <summary>Allows the fire button to charge the equipped beam combination.</summary>
    Charge = 0x1000,
}

/// <summary>
/// Typed queries over raw native equipment words. Storage intentionally remains
/// <see cref="ushort"/> for now so ROM fixtures and debugger-visible WRAM projections keep
/// their exact public shape; only interpretation becomes semantic.
/// </summary>
public static class SamusEquipmentFlagExtensions
{
    /// <summary>Tests whether a native item word contains at least one requested equipment bit.</summary>
    /// <param name="word">Raw collected- or equipped-items word.</param>
    /// <param name="flags">One or more item bits to test.</param>
    /// <returns><see langword="true"/> when the two masks share any set bit.</returns>
    public static bool HasAny(this ushort word, SamusEquipmentFlags flags) =>
        ((SamusEquipmentFlags)word & flags) != 0;

    /// <summary>Tests whether a native beam word contains at least one requested beam component.</summary>
    /// <param name="word">Raw collected- or equipped-beams word.</param>
    /// <param name="flags">One or more beam bits to test.</param>
    /// <returns><see langword="true"/> when the two masks share any set bit.</returns>
    public static bool HasAny(this ushort word, SamusBeamFlags flags) =>
        ((SamusBeamFlags)word & flags) != 0;

    /// <summary>Preserves a typed beam combination as the 16-bit word used by WRAM and cartridge tables.</summary>
    public static ushort ToNativeWord(this SamusBeamFlags flags) => (ushort)flags;

    /// <summary>
    /// Resolves the byte offset used by every native suit-palette pointer table. Gravity
    /// wins when both suit bits are set, exactly matching the cartridge's branch order;
    /// this is an ordinary three-way discriminator, not another flag set.
    /// </summary>
    public static ushort GetSuitPaletteTableOffset(this ushort equippedItems)
    {
        if (equippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return 4;

        return equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
            ? (ushort)2
            : (ushort)0;
    }
}

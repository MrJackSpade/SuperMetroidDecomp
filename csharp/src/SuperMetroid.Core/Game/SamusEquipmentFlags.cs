namespace SuperMetroid.Core.Game;

/// <summary>
/// Independent item bits stored in Samus's native equipped-items word at WRAM
/// <c>$09A2</c>. The missing bit values are unused by the translated retail data and are
/// deliberately not assigned speculative names.
/// </summary>
[Flags]
public enum SamusEquipmentFlags : ushort
{
    None = 0,
    VariaSuit = 0x0001,
    SpringBall = 0x0002,
    MorphBall = 0x0004,
    ScrewAttack = 0x0008,
    GravitySuit = 0x0020,
    HiJumpBoots = 0x0100,
    SpaceJump = 0x0200,
    Bombs = 0x1000,
    SpeedBooster = 0x2000,
    GrappleBeam = 0x4000,
    XrayScope = 0x8000,
}

/// <summary>
/// Independent beam-equipment bits stored at WRAM <c>$09A6</c>. Low-nibble beam bits may
/// be combined; Charge occupies bit twelve just as it does in the cartridge word.
/// </summary>
[Flags]
public enum SamusBeamFlags : ushort
{
    None = 0,
    Wave = 0x0001,
    Ice = 0x0002,
    Spazer = 0x0004,
    Plasma = 0x0008,
    Charge = 0x1000,
}

/// <summary>
/// Typed queries over raw native equipment words. Storage intentionally remains
/// <see cref="ushort"/> for now so ROM fixtures and debugger-visible WRAM projections keep
/// their exact public shape; only interpretation becomes semantic.
/// </summary>
public static class SamusEquipmentFlagExtensions
{
    public static bool HasAny(this ushort word, SamusEquipmentFlags flags) =>
        ((SamusEquipmentFlags)word & flags) != 0;

    public static bool HasAll(this ushort word, SamusEquipmentFlags flags) =>
        ((SamusEquipmentFlags)word & flags) == flags;

    public static bool HasAny(this ushort word, SamusBeamFlags flags) =>
        ((SamusBeamFlags)word & flags) != 0;

    public static bool HasAll(this ushort word, SamusBeamFlags flags) =>
        ((SamusBeamFlags)word & flags) == flags;

    /// <summary>
    /// Returns a native equipment word with the requested, independently combinable bits
    /// enabled. Keeping this operation here prevents call sites from reintroducing casts and
    /// hexadecimal masks merely because the WRAM-facing property remains a <see cref="ushort"/>.
    /// </summary>
    public static ushort With(this ushort word, SamusEquipmentFlags flags) =>
        (ushort)(word | (ushort)flags);

    /// <summary>Returns a native equipment word with the requested bits disabled.</summary>
    public static ushort Without(this ushort word, SamusEquipmentFlags flags) =>
        (ushort)(word & ~(ushort)flags);

    /// <summary>Returns a native beam word with the requested equipment bits enabled.</summary>
    public static ushort With(this ushort word, SamusBeamFlags flags) =>
        (ushort)(word | (ushort)flags);

    /// <summary>Returns a native beam word with the requested equipment bits disabled.</summary>
    public static ushort Without(this ushort word, SamusBeamFlags flags) =>
        (ushort)(word & ~(ushort)flags);

    public static ushort ToNativeWord(this SamusEquipmentFlags flags) => (ushort)flags;

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

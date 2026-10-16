using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Queries over Samus's equipment and beam flags, and their one conversion to the native WRAM/SRAM word (#1323).
/// The enums keep <see cref="ushort"/> as their underlying type, so unnamed bits stay representable.
/// </summary>
public static class SamusEquipmentFlagExtensions
{
    /// <summary>Tests whether an item set contains at least one requested equipment bit.</summary>
    /// <param name="items">Collected or equipped items.</param>
    /// <param name="flags">One or more item bits to test.</param>
    /// <returns><see langword="true"/> when the two sets share any set bit.</returns>
    public static bool HasAny(this SamusEquipmentFlags items, SamusEquipmentFlags flags) => (items & flags) != 0;

    /// <summary>Tests whether a beam set contains at least one requested beam component.</summary>
    /// <param name="beams">Collected or equipped beams.</param>
    /// <param name="flags">One or more beam bits to test.</param>
    /// <returns><see langword="true"/> when the two sets share any set bit.</returns>
    public static bool HasAny(this SamusBeamFlags beams, SamusBeamFlags flags) => (beams & flags) != 0;

    /// <summary>The item set as the 16-bit word WRAM, SRAM and cartridge tables use.</summary>
    public static ushort ToNativeWord(this SamusEquipmentFlags items) => (ushort)items;

    /// <summary>The beam set as the 16-bit word WRAM, SRAM and cartridge tables use.</summary>
    public static ushort ToNativeWord(this SamusBeamFlags beams) => (ushort)beams;

    /// <summary>
    /// Resolves the byte offset used by every native suit-palette pointer table. Gravity
    /// wins when both suit bits are set, exactly matching the cartridge's branch order;
    /// this is an ordinary three-way discriminator, not another flag set.
    /// </summary>
    public static ushort GetSuitPaletteTableOffset(this SamusEquipmentFlags equippedItems)
    {
        if (equippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return 4;

        return equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
            ? (ushort)2
            : (ushort)0;
    }
}

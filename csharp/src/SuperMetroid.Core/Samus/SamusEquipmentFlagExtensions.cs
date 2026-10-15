using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

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

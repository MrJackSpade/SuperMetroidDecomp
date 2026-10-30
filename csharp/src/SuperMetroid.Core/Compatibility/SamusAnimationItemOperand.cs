using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The item an item-selected Samus animation transition tests, as two bytes of the animation delay stream. The
/// interpreter reads the stream by position, and where an animation frame runs past its program it reads the two
/// bytes from whatever lies there; the items those bytes name decide which transitional pose Samus takes next, so the
/// item bit each byte position stands for is gameplay-observable.
/// </summary>
/// <remarks>
/// Consulted by the animation interpreter's item transition commands each time one runs, and by the delay stream when
/// it lays an item operand into its bytes. The original game stored the tested items as an equipment bit mask in the
/// command's operand; the Samus animation command checks and the morph-entry transitions confirm the item each bit
/// selects.
/// </remarks>
[CompatibilityService("an animation item transition tests the items named by the two stream bytes it reads, even past its program")]
[EncodingOwner("Samus animation item operand")]
public static class SamusAnimationItemOperand
{
    /// <summary>An item and the operand bit that names it.</summary>
    /// <param name="Item">The item.</param>
    /// <param name="Bit">Its bit in the operand.</param>
    private readonly record struct ItemBit(SamusEquipmentFlags Item, int Bit);

    /// <summary>Each item and the operand bit that names it; the other bits name no item.</summary>
    private static readonly ItemBit[] ItemBits =
    [
        new(SamusEquipmentFlags.VariaSuit, 1 << 0),
        new(SamusEquipmentFlags.SpringBall, 1 << 1),
        new(SamusEquipmentFlags.MorphBall, 1 << 2),
        new(SamusEquipmentFlags.ScrewAttack, 1 << 3),
        new(SamusEquipmentFlags.UnassignedItem, 1 << 4),
        new(SamusEquipmentFlags.GravitySuit, 1 << 5),
        new(SamusEquipmentFlags.HiJumpBoots, 1 << 8),
        new(SamusEquipmentFlags.SpaceJump, 1 << 9),
        new(SamusEquipmentFlags.Bombs, 1 << 12),
        new(SamusEquipmentFlags.SpeedBooster, 1 << 13),
        new(SamusEquipmentFlags.GrappleBeam, 1 << 14),
        new(SamusEquipmentFlags.XrayScope, 1 << 15),
    ];

    /// <summary>The operand that names <paramref name="items"/>, 0 through 65535.</summary>
    public static int Of(SamusEquipmentFlags items)
    {
        int operand = 0;
        foreach (ItemBit entry in ItemBits)
        {
            if ((items & entry.Item) != SamusEquipmentFlags.None)
                operand |= entry.Bit;
        }
        return operand;
    }

    /// <summary>Whether <paramref name="equipped"/> holds any item <paramref name="operand"/> names.</summary>
    /// <param name="equipped">Samus's equipped items.</param>
    /// <param name="operand">The two stream bytes read as an operand, 0 through 65535.</param>
    public static bool HoldsAny(SamusEquipmentFlags equipped, int operand) => (Of(equipped) & operand) != 0;
}

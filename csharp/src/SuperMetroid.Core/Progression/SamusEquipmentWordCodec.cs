namespace SuperMetroid.Core.Progression;

/// <summary>
/// The codec between Samus's typed equipment and beam flags and the sixteen-bit equipment words that save slots,
/// Samus's animation item masks and the projectile type record store. The item flags' values are arbitrary; this type
/// is the one place that knows each item's bit in the word, and the one place either flag set becomes that word.
/// </summary>
[EncodingOwner("save slot equipment words")]
public static class SamusEquipmentWordCodec
{
    /// <summary>An item and the word bit that records it.</summary>
    /// <param name="Item">The item.</param>
    /// <param name="Bit">Its bit in the equipment word.</param>
    private readonly record struct ItemBit(SamusEquipmentFlags Item, ushort Bit);

    /// <summary>Each item and the word bit that records it.</summary>
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

    /// <summary>The item set as its sixteen-bit equipment word.</summary>
    /// <param name="items">Collected or equipped items.</param>
    /// <returns>The items' bits as one word.</returns>
    public static ushort ToEquipmentWord(this SamusEquipmentFlags items)
    {
        ushort word = 0;
        foreach (ItemBit entry in ItemBits)
        {
            if ((items & entry.Item) != 0)
                word |= entry.Bit;
        }
        return word;
    }

    /// <summary>The items an equipment word records.</summary>
    /// <param name="word">A sixteen-bit equipment word.</param>
    /// <returns>The items whose bits are set.</returns>
    /// <exception cref="InvalidDataException">The word sets a bit that records no item.</exception>
    public static SamusEquipmentFlags FromEquipmentWord(ushort word)
    {
        SamusEquipmentFlags items = SamusEquipmentFlags.None;
        ushort recorded = 0;
        foreach (ItemBit entry in ItemBits)
        {
            if ((word & entry.Bit) != 0)
                items |= entry.Item;
            recorded |= entry.Bit;
        }
        if ((word & ~recorded) != 0)
            throw new InvalidDataException($"Equipment word ${word:X4} sets bits ${word & ~recorded:X4}, which record no item.");
        return items;
    }

    /// <summary>The beam set as its sixteen-bit equipment word.</summary>
    /// <param name="beams">Collected or equipped beams.</param>
    /// <returns>The beams' bits as one word.</returns>
    public static ushort ToEquipmentWord(this SamusBeamFlags beams) => (ushort)beams;
}

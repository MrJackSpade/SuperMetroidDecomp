using SuperMetroid.Core;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec between Samus's typed equipment and beam flags and the sixteen-bit equipment words save slots and the
/// cartridge's work memory store. The flags' values are arbitrary; this type is the one place that knows each item's
/// and beam's bit in its word, for the save import and export and for checks that compare with native state.
/// </summary>
[EncodingOwner("save slot equipment words")]
public static class SamusEquipmentWordCodec
{
    /// <summary>An item and the word bit that records it.</summary>
    /// <param name="Item">The item.</param>
    /// <param name="Bit">Its bit in the equipment word.</param>
    private readonly record struct ItemBit(SamusEquipmentFlags Item, ushort Bit);

    /// <summary>A beam and the word bit that records it.</summary>
    /// <param name="Beam">The beam component or upgrade.</param>
    /// <param name="Bit">Its bit in the beam word.</param>
    private readonly record struct BeamBit(SamusBeamFlags Beam, ushort Bit);

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

    /// <summary>Each beam and the word bit that records it.</summary>
    private static readonly BeamBit[] BeamBits =
    [
        new(SamusBeamFlags.Wave, 1 << 0),
        new(SamusBeamFlags.Ice, 1 << 1),
        new(SamusBeamFlags.Spazer, 1 << 2),
        new(SamusBeamFlags.Plasma, 1 << 3),
        new(SamusBeamFlags.Charge, 1 << 12),
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

    /// <summary>The bits of an equipment word that record an item.</summary>
    private static readonly ushort ItemWordBits = ItemBits.Aggregate((ushort)0, (bits, entry) => (ushort)(bits | entry.Bit));

    /// <summary>
    /// The items a save slot's stored equipment word records (#1456). The cartridge accepts a checksum-valid slot whatever
    /// else the word sets and never reads a bit that records no item, so those bits stand for nothing and are not carried:
    /// re-encoding the slot writes them as zero.
    /// </summary>
    /// <param name="word">A save slot's sixteen-bit equipment word.</param>
    /// <returns>The items whose bits are set.</returns>
    public static SamusEquipmentFlags FromSavedEquipmentWord(ushort word) => FromEquipmentWord((ushort)(word & ItemWordBits));

    /// <summary>
    /// The beams a save slot's stored beam word records (#1456). As with <see cref="FromSavedEquipmentWord"/>, bits that
    /// record no beam are never read by the cartridge and are not carried.
    /// </summary>
    /// <param name="word">A save slot's sixteen-bit beam word.</param>
    /// <returns>The beams whose bits are set.</returns>
    public static SamusBeamFlags FromSavedBeamWord(ushort word)
    {
        SamusBeamFlags beams = SamusBeamFlags.None;
        foreach (BeamBit entry in BeamBits)
        {
            if ((word & entry.Bit) != 0)
                beams |= entry.Beam;
        }
        return beams;
    }

    /// <summary>The beam set as its sixteen-bit equipment word.</summary>
    /// <param name="beams">Collected or equipped beams.</param>
    /// <returns>The beams' bits as one word.</returns>
    public static ushort ToEquipmentWord(this SamusBeamFlags beams)
    {
        ushort word = 0;
        foreach (BeamBit entry in BeamBits)
        {
            if ((beams & entry.Beam) != 0)
                word |= entry.Bit;
        }
        return word;
    }
}

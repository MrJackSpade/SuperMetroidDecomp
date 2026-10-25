namespace SuperMetroid.Core.Progression;

/// <summary>
/// The codec between Samus's typed equipment and beam flags and the sixteen-bit equipment words that save slots,
/// Samus's animation item masks and the projectile type record store. The enums keep <see cref="ushort"/> as their
/// underlying type, so unnamed bits stay representable; this type is the one place either flag set becomes that word.
/// </summary>
[SnesEncodingOwner]
public static class SamusEquipmentWordCodec
{
    /// <summary>The item set as its sixteen-bit equipment word.</summary>
    /// <param name="items">Collected or equipped items.</param>
    /// <returns>The items' bits as one word.</returns>
    public static ushort ToEquipmentWord(this SamusEquipmentFlags items) => (ushort)items;

    /// <summary>The beam set as its sixteen-bit equipment word.</summary>
    /// <param name="beams">Collected or equipped beams.</param>
    /// <returns>The beams' bits as one word.</returns>
    public static ushort ToEquipmentWord(this SamusBeamFlags beams) => (ushort)beams;
}

namespace SuperMetroid.Core.Progression;

/// <summary>
/// The codec between Samus's typed equipment and beam flags and the 16-bit item and beam words the save RAM layout,
/// the native snapshot and the remaining word-shaped projectile records store. The enums keep <see cref="ushort"/> as
/// their underlying type, so unnamed bits stay representable; this type is the one place either flag set becomes that
/// word.
/// </summary>
[SnesEncodingOwner]
public static class SamusEquipmentWordCodec
{
    /// <summary>The item set as the 16-bit word the save layout and native snapshot store.</summary>
    /// <param name="items">Collected or equipped items.</param>
    /// <returns>The items' bits as one word.</returns>
    public static ushort ToNativeWord(this SamusEquipmentFlags items) => (ushort)items;

    /// <summary>The beam set as the 16-bit word the save layout and native snapshot store.</summary>
    /// <param name="beams">Collected or equipped beams.</param>
    /// <returns>The beams' bits as one word.</returns>
    public static ushort ToNativeWord(this SamusBeamFlags beams) => (ushort)beams;
}

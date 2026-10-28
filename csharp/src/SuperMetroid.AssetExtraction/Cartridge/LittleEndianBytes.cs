using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// The cartridge's little-endian byte order (#223, #1422): a 16-bit word stores its low byte first, and a 24-bit long adds its
/// bank byte third. This type owns splitting values into those byte lanes and composing them back. Byte sources that
/// are not CPU address spaces (cartridge tables, routed WRAM/ROM windows, test byte stores) compose their words here;
/// address-space reads use <see cref="CartridgeWordReads"/>. It belongs to the import pipeline and its verification:
/// gameplay keeps no storage bytes, so it names each split for what the game means by it instead.
/// </summary>
/// <remarks>
/// Use these only where the value really is split into storage bytes: a word written to or read from byte-wide
/// memory, a register's low/high byte, or a byte the cartridge stores in a word's high half. A shift that divides
/// by 256 for another reason (an 8.8 whole part, a 256-pixel screen, a fraction scale) belongs to that domain's type.
/// </remarks>
[EncodingOwner("little-endian byte order of stored words")]
public static class LittleEndianBytes
{
    /// <summary>Bits in one storage byte, the distance between adjacent byte lanes.</summary>
    public const int BitsPerByte = 8;

    /// <summary>The 16-bit word whose low byte is <paramref name="low"/> and high byte is <paramref name="high"/>.</summary>
    public static ushort Word(byte low, byte high) => (ushort)(low | high << BitsPerByte);

    /// <summary>The byte stored first (at the word's address): a packed word's low byte field.</summary>
    public static byte LowByte(ushort word) => unchecked((byte)word);

    /// <summary>The low byte field of a packed word held as an int, 0 through 65535; other values are rejected.</summary>
    public static byte LowByte(int word) => LowByte(RequireWord(word));

    /// <summary>A packed word's high byte field kept in place, the low byte cleared.</summary>
    public static int HighByteInPlace(ushort word) => word & 0xff00;

    /// <summary>The byte stored second (at the word's address + 1).</summary>
    public static byte HighByte(ushort word) => unchecked((byte)(word >> BitsPerByte));

    /// <summary>The high byte field of a packed word held as an int, 0 through 65535; other values are rejected.</summary>
    public static byte HighByte(int word) => HighByte(RequireWord(word));

    /// <summary>A word held as an int, checked to lie in 0 through 65535.</summary>
    private static ushort RequireWord(int word) => checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, word));
}

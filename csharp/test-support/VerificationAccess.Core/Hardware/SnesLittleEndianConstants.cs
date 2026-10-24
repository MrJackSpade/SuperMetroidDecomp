namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="SnesLittleEndian"/> that only verification reads.</summary>
internal static class SnesLittleEndianConstants
{
    /// <summary>Bits in one storage word, the distance from a long's low word to its bank byte.</summary>
    public const int BitsPerWord = 16;
    /// <summary>The third byte of a 24-bit long: the bank byte of a long pointer.</summary>
    public static byte BankByte(int value) => unchecked((byte)(value >> BitsPerWord));
}

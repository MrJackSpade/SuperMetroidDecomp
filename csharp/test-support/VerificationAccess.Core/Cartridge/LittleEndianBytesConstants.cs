namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>Cartridge values and helpers of <see cref="LittleEndianBytes"/> that only verification reads.</summary>
internal static class LittleEndianBytesConstants
{
    /// <summary>The word with its two bytes exchanged, as the 65816's <c>XBA</c> does.</summary>
    public static ushort SwapBytes(ushort word) => LittleEndianBytes.Word(LittleEndianBytes.HighByte(word), unchecked((byte)word));
}

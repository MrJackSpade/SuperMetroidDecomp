namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The SNES's little-endian byte order (#223): a 16-bit word stores its low byte first. Byte sources that are
/// not CPU address spaces (compiled definition tables, routed WRAM/ROM windows, test byte stores) compose
/// their words here; address-space reads use <see cref="SnesWordReads"/> and the cartridge word reads.
/// </summary>
public static class SnesLittleEndian
{
    /// <summary>The 16-bit word whose low byte is <paramref name="low"/> and high byte is <paramref name="high"/>.</summary>
    public static ushort Word(byte low, byte high) => (ushort)(low | high << 8);
}

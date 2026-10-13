namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The canonical 16-bit cartridge reads for asset import and verification (#223), the cartridge
/// counterpart of <see cref="SnesWordReads"/>. The SNES stores words
/// little-endian: the low byte at the word's address, the high byte at the next address. Each operation
/// reads exactly two bytes, low then high, once each, and propagates the source's failures unchanged.
/// </summary>
/// <remarks>
/// Two address progressions exist and are not interchangeable at a bank's last byte:
/// <list type="bullet">
/// <item><description><b>Linear</b> (<c>Read…Word</c>): the high byte comes from the next 24-bit bus address,
/// so a word at <c>$xx:FFFF</c> takes its high byte from <c>$(xx+1):0000</c>. This is a long (24-bit) access.</description></item>
/// <item><description><b>Fixed bank</b> (<c>Read…WordFixedBank</c>): the high byte stays in the word's bank,
/// so a word at <c>$xx:FFFF</c> takes its high byte from <c>$xx:0000</c>. This is the 65816's 16-bit
/// data-bank addressing, used by bank-relative tables and pointers.</description></item>
/// </list>
/// The two read the same bytes everywhere else.
/// </remarks>
[CpuBusAccess]
[SnesEncodingOwner]
public static class CartridgeWordReads
{
    /// <summary>Adds cartridge byte-combination operations to import sources; fixed-bank variants wrap at $FFFF.</summary>
    extension(IImportCartridgeSource cartridge)
    {
        /// <summary>Reads the little-endian cartridge word at <paramref name="cpuAddress"/> on the linear 24-bit bus.</summary>
        public ushort ReadCartridgeWord(int cpuAddress)
        {
            Ensure.NotNull(cartridge);
            byte low = cartridge.ReadCartridgeByte(cpuAddress);
            return (ushort)(low | cartridge.ReadCartridgeByte(cpuAddress + 1) << 8);
        }


        /// <summary>Reads the little-endian 24-bit cartridge value at <paramref name="cpuAddress"/>, every byte staying in the bank.</summary>
        public int ReadCartridgeLongFixedBank(int cpuAddress)
        {
            ushort low = cartridge.ReadCartridgeWordFixedBank(cpuAddress);
            return low | cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(cpuAddress, 2)) << 16;
        }

        /// <summary>Reads the little-endian cartridge word at <paramref name="cpuAddress"/>, its high byte wrapping within the bank.</summary>
        public ushort ReadCartridgeWordFixedBank(int cpuAddress)
        {
            Ensure.NotNull(cartridge);
            byte low = cartridge.ReadCartridgeByte(cpuAddress);
            return (ushort)(low | cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(cpuAddress, 1)) << 8);
        }
    }
}

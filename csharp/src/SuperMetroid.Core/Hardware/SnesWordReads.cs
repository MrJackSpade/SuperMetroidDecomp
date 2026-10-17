namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The canonical 16-bit reads over WRAM (#223); cartridge words are
/// <c>CartridgeWordReads</c> in the import assembly. The SNES stores words
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
public static class SnesWordReads
{
    /// <summary>
    /// Adds WRAM word reads to the mutable CPU memory boundary while preserving the
    /// linear-versus-fixed-bank address progression described above.
    /// </summary>
    extension(ISnesMutableMemory memory)
    {

        /// <summary>Reads the little-endian WRAM word at <paramref name="cpuAddress"/>, its high byte wrapping within the bank.</summary>
        public ushort ReadWorkRamWordFixedBank(int cpuAddress)
        {
            Ensure.NotNull(memory);
            byte low = memory.ReadWorkRamByte(cpuAddress);
            return (ushort)(low | memory.ReadWorkRamByte(SnesAddressMath.AddWithinBank(cpuAddress, 1)) << 8);
        }

    }
}

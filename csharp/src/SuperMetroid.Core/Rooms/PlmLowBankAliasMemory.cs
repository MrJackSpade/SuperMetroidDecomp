using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The live WRAM a bank-$84 PLM instruction or draw-list pointer reaches when it wraps below
/// $2000 (#629). <c>$84:0000-$84:1FFF</c> is the low-WRAM mirror, never cartridge data.
/// </summary>
[CpuBusBoundary("domain view: PLM bank-$84 pointers that wrap into the low WRAM mirror")]
internal readonly struct PlmLowBankAliasMemory(ISnesMutableMemory memory)
{
    /// <summary>The word at bank-$84 <paramref name="offset"/>: low byte first, the high byte wrapping within the bank.</summary>
    internal ushort Word(ushort offset) =>
        SnesWorkRam.ReadWord(memory, (int)new SnesAddress(RoomPlmMemoryLayout.ProgramBank, offset));

    /// <summary>The byte at bank-$84 <paramref name="offset"/>.</summary>
    internal byte Byte(ushort offset) =>
        memory.ReadWorkRamByte((int)new SnesAddress(RoomPlmMemoryLayout.ProgramBank, offset));
}

namespace SuperMetroid.Core.Hardware;

/// <summary>The mutually exclusive mapped sources a PPU DMA transfer can read.</summary>
internal enum SnesDmaSourceKind
{
    WorkRam,
    SaveRam,
    Cartridge,
    Unmapped,
}

/// <summary>
/// Classifies each A-bus byte after the DMA channel's 16-bit offset wraps. A transfer
/// that crosses $xx:FFFF can change from a cartridge window to WRAM or an unmapped
/// window without changing its bank register.
/// </summary>
internal static class SnesDmaSourceMap
{
    internal static SnesDmaSourceKind Classify(SnesAddress address)
    {
        byte bank = address.Bank;
        ushort offset = address.Offset;
        if (bank is >= LoRomMemoryBankLayout.FirstWorkRamBank and
            <= LoRomMemoryBankLayout.LastWorkRamBank)
            return SnesDmaSourceKind.WorkRam;
        if ((bank & LoRomExpansionReadMap.MirrorBankMask) < LoRomExpansionReadMap.SystemBankLimit &&
            offset < LoRomExpansionReadMap.WorkRamMirrorEnd)
            return SnesDmaSourceKind.WorkRam;
        if ((bank is >= LoRomMemoryBankLayout.FirstSaveRamLowBank and
                <= LoRomMemoryBankLayout.LastSaveRamLowBank or
                 >= LoRomMemoryBankLayout.FirstSaveRamHighBank and
                <= LoRomMemoryBankLayout.LastSaveRamHighBank) &&
            offset < LoRomExpansionReadMap.RomStart)
            return SnesDmaSourceKind.SaveRam;
        return offset >= LoRomExpansionReadMap.RomStart
            ? SnesDmaSourceKind.Cartridge
            : SnesDmaSourceKind.Unmapped;
    }
}

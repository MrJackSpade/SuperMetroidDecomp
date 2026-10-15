namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Classifies each A-bus byte after the DMA channel's 16-bit offset wraps. A transfer
/// that crosses $xx:FFFF can change from a cartridge window to WRAM or an unmapped
/// window without changing its bank register.
/// </summary>
internal static class SnesDmaSourceMap
{
    /// <summary>Classifies one DMA A-bus address after offset wrapping, honoring direct WRAM, mirrors, SRAM, and LoROM windows.</summary>
    internal static SnesDmaSourceKind Classify(SnesAddress address)
    {
        byte bank = address.Bank;
        ushort offset = address.Offset;
        if (bank is >= LoRomMemoryBankLayout.FirstWorkRamBank and
            <= LoRomMemoryBankLayout.LastWorkRamBank)
            return SnesDmaSourceKind.WorkRam;
        if ((bank & SnesBusLayout.MirrorBankMask) < SnesBusLayout.SystemBankLimit &&
            offset < SnesBusLayout.WorkRamMirrorEnd)
            return SnesDmaSourceKind.WorkRam;
        if ((bank is >= LoRomMemoryBankLayout.FirstSaveRamLowBank and
                <= LoRomMemoryBankLayout.LastSaveRamLowBank or
                 >= LoRomMemoryBankLayout.FirstSaveRamHighBank and
                <= LoRomMemoryBankLayout.LastSaveRamHighBank) &&
            offset < SnesBusLayout.RomWindowStart)
            return SnesDmaSourceKind.SaveRam;
        return offset >= SnesBusLayout.RomWindowStart
            ? SnesDmaSourceKind.Cartridge
            : SnesDmaSourceKind.Unmapped;
    }
}

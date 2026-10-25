namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>Full WRAM banks and LoROM lower-window SRAM bank ranges.</summary>
internal static class LoRomMemoryBankLayout
{
    /// <summary>First full WRAM bank, $7E.</summary>
    internal const byte FirstWorkRamBank = 0x7e;
    /// <summary>Last full WRAM bank, $7F.</summary>
    internal const byte LastWorkRamBank = 0x7f;
}

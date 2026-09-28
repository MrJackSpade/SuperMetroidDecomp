namespace SuperMetroid.Core.Hardware;

/// <summary>Full WRAM banks and LoROM lower-window SRAM bank ranges.</summary>
internal static class LoRomMemoryBankLayout
{
    /// <summary>First full WRAM bank, $7E.</summary>
    internal const byte FirstWorkRamBank = 0x7e;
    /// <summary>Last full WRAM bank, $7F.</summary>
    internal const byte LastWorkRamBank = 0x7f;
    /// <summary>First lower-window SRAM bank, $70.</summary>
    internal const byte FirstSaveRamLowBank = 0x70;
    /// <summary>Last lower-window SRAM bank, $7D.</summary>
    internal const byte LastSaveRamLowBank = 0x7d;
    /// <summary>First high mirrored SRAM bank, $F0.</summary>
    internal const byte FirstSaveRamHighBank = 0xf0;
    /// <summary>Last high mirrored SRAM bank, $FF.</summary>
    internal const byte LastSaveRamHighBank = 0xff;
}

namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="PpuMemoryLayout"/> that only verification reads.</summary>
internal static class PpuMemoryLayoutConstants
{
    /// <summary>All 128 OAM low-table records ($2104 OAMDATA, OAMADD $000-$0FF words).</summary>
    public static readonly PpuLayoutCell OamLow = new(nameof(OamLow), PpuLayoutRegion.OamLow, 0, SnesPpuLayout.OamSpriteCount,
        "OBJ records (X low, Y, SnesObjAttributeWord)");
    /// <summary>All 128 OAM high-table pairs (OAMADD $100-$10F words): X bit 8 and the large-size bit.</summary>
    public static readonly PpuLayoutCell OamHigh = new(nameof(OamHigh), PpuLayoutRegion.OamHigh, 0, SnesPpuLayout.OamSpriteCount,
        "SnesOamHighTablePair (2 bits per OBJ)");
}

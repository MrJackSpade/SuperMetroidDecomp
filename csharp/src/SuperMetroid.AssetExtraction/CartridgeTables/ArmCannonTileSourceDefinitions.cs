namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge keeps the arm cannon's cover tiles (#1411).</summary>
[SuperMetroid.Core.Hardware.CpuBusBoundary("catalog: arm cannon cover tile sources")]
internal static class ArmCannonTileSourceDefinitions
{
    /// <summary>$9A:9A00, Tiles_NonClosed_ArmCannon_Vertical_0, the bank-$9A source of the first of twelve cover tiles.</summary>
    internal const int FirstTileSource = 0x9a00;
}

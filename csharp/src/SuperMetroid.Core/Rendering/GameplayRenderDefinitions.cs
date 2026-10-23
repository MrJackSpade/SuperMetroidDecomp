namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge register definitions for gameplay display capture.</summary>
internal static class GameplayRenderDefinitions
{
    /// <summary>OBSEL=$03: gameplay OBJ uses word $6000, with 8/16-pixel object sizes.</summary>
    internal const byte ObjectSelection = 0x03;
    /// <summary>Bank-$88 Ceres Ridley getaway HDMA returns to Mode 1 for the final sixteen visible scanlines.</summary>
    internal const int CeresFloorFirstScanline = Hardware.SnesPpuLayout.ScreenHeightPixels - 16;
}

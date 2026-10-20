namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values of <see cref="SnesPpuLayout"/> that only verification reads.</summary>
internal static class SnesPpuLayoutConstants
{
    /// <summary>Room scanlines below the HUD.</summary>
    public const int GameplayViewportHeightPixels = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;

    /// <summary>Provides the gameplay viewport height used in SNES PPU layout comparisons.</summary>
    extension(SnesPpuLayout)
    {
        /// <inheritdoc cref="SnesPpuLayoutConstants.GameplayViewportHeightPixels"/>
        internal static int GameplayViewportHeightPixels => SnesPpuLayoutConstants.GameplayViewportHeightPixels;
    }
    /// <summary>Size of OAM's two-bits-per-OBJ high table.</summary>
    [PpuLayoutAccess]
    public const int OamHighTableByteCount = SnesPpuLayout.OamSpriteCount / 4;
    /// <summary>Complete low-plus-high OAM DMA payload.</summary>
    [PpuLayoutAccess]
    public const int OamUploadByteCount = SnesPpuLayoutConstants.OamLowTableByteCount + OamHighTableByteCount;
    /// <summary>Size of OAM's four-byte-per-OBJ low table.</summary>
    [PpuLayoutAccess]
    public const int OamLowTableByteCount = SnesPpuLayout.OamSpriteCount * 4;
}

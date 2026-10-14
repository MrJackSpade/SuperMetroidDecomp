using SuperMetroid.Core.Hardware;

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
}

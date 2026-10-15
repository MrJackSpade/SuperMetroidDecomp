using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="GameplayMessageBoxRenderer"/> that only verification reads.</summary>
internal static class GameplayMessageBoxRendererConstants
{
    /// <summary>
    /// Draws the currently exposed scanline band. The original uses BG3 vertical-scroll
    /// HDMA to reveal a centered box; evaluating the resulting clipped raster directly is
    /// equivalent and avoids mutating the gameplay HUD tilemap in modeled VRAM.
    /// </summary>
    public static void Composite(
        Span<Rgba32> frame,
        GameplayMessageBoxState messageBox,
        SnesVram vram,
        SnesCgram cgram)
    {
        Ensure.NotNull(messageBox);
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.LengthEqual(frame, GameplayMessageBoxRenderer.ScreenWidth * GameplayMessageBoxRenderer.ScreenHeight);
        if (!messageBox.IsActive || messageBox.RadiusPixels == 0)
            return;

        ReadOnlySpan<ushort> tilemap = messageBox.Tilemap;
        int rowCount = messageBox.TilemapRowCount;
        GameplayMessageBoxRenderer.CompositeTilemap(frame, tilemap, rowCount, messageBox.RadiusPixels, vram, cgram);
    }
}

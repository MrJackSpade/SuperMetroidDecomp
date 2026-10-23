using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="SnesBgTilemapRenderer"/> members production does not use.</summary>
internal static class SnesBgTilemapRendererAccess
{
    /// <summary>Creates owned pixel buffers for the background renderer's 4-bpp and 2-bpp verification surfaces.</summary>
    extension(SnesBgTilemapRenderer)
    {
        /// <summary>
        /// Renders a pixel-scrolled Mode-1 4-bpp viewport from any BGSC 32/64-by-32/64 map.
        /// Palette index zero is transparent so a lower-priority BG or backdrop remains visible.
        /// </summary>
        internal static Rgba32[] Render4BppViewport(
            SnesVram vram,
            SnesCgram cgram,
            BgVramAddresses addresses,
            BgViewport viewport,
            bool? priority = null)
        {
            var output = new Rgba32[viewport.Size.X * viewport.Size.Y];
            SnesBgTilemapRenderer.Composite4BppViewport(output, vram, cgram, addresses, viewport, priority);
            return output;
        }

        /// <summary>
        /// Renders consecutive tilemap rows. This first implementation supports the 2-bpp BG3
        /// format used by the gameplay HUD; its parameters retain the actual VRAM bases so the
        /// debugger can be compared directly with PPU registers.
        /// </summary>
        internal static Rgba32[] Render2Bpp(
            SnesVram vram,
            SnesCgram cgram,
            int tilemapBaseWord,
            int characterBaseWord,
            int rowCount,
            bool transparentColorZero = false,
            bool? priority = null)
        {
            const int width = 32 * 8;
            int height = rowCount * 8;
            var output = new Rgba32[width * height];
            SnesBgTilemapRendererConstants.Render2Bpp(output, vram, cgram, new BgVramAddresses { TilemapBaseWord = tilemapBaseWord, CharacterBaseWord = characterBaseWord }, rowCount, transparentColorZero, priority);
            return output;
        }
    }
}

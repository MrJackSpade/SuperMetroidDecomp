using SuperMetroid.Core.Hardware;
﻿using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectAreaMapGraphics"/> members production does not use.</summary>
internal static class FileSelectAreaMapGraphicsAccess
{
    /// <summary>Exposes <see cref="FileSelectAreaMapGraphics"/> state that only verification reads.</summary>
    extension(FileSelectAreaMapGraphics self)
    {
        /// <summary>Composes the unmasked 256-by-224 BG1 main scene plus additive BG3 subscreen, excluding OBJ labels and transition clipping; no menu state advances.</summary>
        /// <param name="includeBackdropInColorMath">True for steady/return $25 color math; false for forward-window $05 behavior, where transparent BG1 cells retain the backdrop without BG3 addition.</param>
        /// <returns>The owned RGBA frame, valid until this graphics owner renders again.</returns>
        [PpuLayoutBoundary("backend: file-select area-map software PPU composition")]
        public Rgba32[] RenderBackgrounds(bool includeBackdropInColorMath = true)
        {
            Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(self.ppu.Cgram, 256 * 224, new Rgba32[256 * 224]);
            Rgba32[] foreground = new Rgba32[256 * 224];
            foreground.AsSpan().Clear();
            SnesBgTilemapRenderer.Composite4BppViewport(foreground, self.ppu.Vram, self.ppu.Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg1TilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, 32) });
            SnesLayerCompositor.Composite(pixels, foreground);
            Rgba32[] subscreen = new Rgba32[256 * 28 * 8];
            subscreen.AsSpan().Clear();
            SnesBgTilemapRenderer.Render2Bpp(subscreen, self.ppu.Vram, self.ppu.Cgram, new BgVramAddresses { TilemapBaseWord = FileSelectMapRomData.AreaBackgroundVram, CharacterBaseWord = FileSelectMapRomData.AreaBackgroundCharacters }, 28, transparentColorZero: true);
            // $81:AAAC changes CGADSUB from $25 to $05: BG1 still adds BG3, but
            // transparent BG1 cells now retain the backdrop instead of adding BG3 to it.
            if (!includeBackdropInColorMath)
                for (int pixel = 0; pixel < subscreen.Length; pixel++)
                    if (foreground[pixel].A == 0) subscreen[pixel] = default;
            SnesLayerCompositor.AddSubscreen(pixels, subscreen);
            return pixels;
        }
    }
}

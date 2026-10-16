using SuperMetroid.Core.Hardware;
﻿using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectRoomMapGraphics"/> members production does not use.</summary>
internal static class FileSelectRoomMapGraphicsAccess
{
    /// <summary>Exposes <see cref="FileSelectRoomMapGraphics"/> state that only verification reads.</summary>
    extension(FileSelectRoomMapGraphics self)
    {
        /// <summary>BG2-only endpoint of $81:AC2D; room-map cells are not installed until $81:AD17.</summary>
        /// <returns>Owned 256-by-224 RGBA frame buffer, valid until this method renders again; BG2 uses fixed scroll (0, 24), with no map icons or timing updates.</returns>
        [PpuLayoutBoundary("backend: file-select room-map software PPU composition")]
        public Rgba32[] RenderFrameOnly()
        {
            Rgba32[] pixels = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
            Array.Fill(pixels, new CgramPalettes(self.Cgram).BackdropRgba);
            // Composite directly: transparent cells leave the backdrop, as compositing a plane would.
            SnesBgTilemapRenderer.Composite4BppViewport(pixels, self.Vram, self.Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg2TilemapWord, CharacterBaseWord = FileSelectMapRomData.RoomCharacters }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 24), Size = new Extent16<ScreenPixels>(FrontendFrame.Width, FrontendFrame.Height), TilemapSize = new Extent16<TilemapCells>(32, 32) });
            return pixels;
        }
        /// <summary>Renders Mode-1 BG priorities with independent scrolling for map and fixed frame.</summary>
        /// <param name="scroll">BG1 scroll-register value in pixels.</param>
        /// <returns>Owned 256-by-224 RGBA buffer, overwritten by the next background or combined render; within each low/high priority tier the fixed BG2 frame precedes BG1 map cells.</returns>
        [PpuLayoutBoundary("backend: file-select room-map software PPU composition")]
        public Rgba32[] RenderBackgrounds(Position16<MapPixels> scroll)
        {
            Rgba32[] pixels = new Rgba32[256 * 224];
            Array.Fill(pixels, new CgramPalettes(self.ppu.Cgram).BackdropRgba);
            for (int tier = 0; tier < 2; tier++)
            {
                // Mode 1 orders BG2 before BG1 within each background priority tier.
                bool priority = tier == 1;
                // Composited directly into the frame; transparent pixels leave it untouched.
                SnesBgTilemapRenderer.Composite4BppViewport(pixels, self.Vram, self.Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg2TilemapWord, CharacterBaseWord = FileSelectMapRomData.RoomCharacters }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 24), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
                SnesBgTilemapRenderer.Composite4BppViewport(pixels, self.Vram, self.Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg1TilemapWord, CharacterBaseWord = FileSelectMapRomData.RoomCharacters }, new BgViewport { Scroll = scroll.AsBgPlaneScroll(), Size = SnesPpuLayout.ScreenSize }, priority: priority);
            }
            return pixels;
        }
    }
}

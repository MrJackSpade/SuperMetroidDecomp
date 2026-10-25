using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access: the file-select map graphics' pixels through their published snapshots.</summary>
internal static class FileSelectMapGraphicsPixelsAccess
{
    /// <summary>The area map's published pixels.</summary>
    extension(FileSelectAreaMapGraphics graphics)
    {
        /// <summary>Captures the area map for the visited-station masks and draws it with the game's renderer.</summary>
        internal RgbaPixel[] Render(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true) =>
            FrontendScenePixelsAccess.Pixels(graphics.CaptureRenderSnapshot(usedStationMasks, ColorMath(includeBackdropInColorMath)));

        /// <summary>
        /// The area map's BG1 foreground with its additive BG3 background and no labels, drawn with the game's renderer.
        /// </summary>
        /// <param name="includeBackdropInColorMath">True for steady/return $25 color math; false for forward-window $05 behavior, where transparent BG1 cells retain the backdrop without BG3 addition.</param>
        internal RgbaPixel[] RenderBackgrounds(bool includeBackdropInColorMath = true) =>
            FrontendScenePixelsAccess.Pixels(graphics.CaptureBackgrounds(ColorMath(includeBackdropInColorMath)));
    }

    /// <summary>The room map's published pixels.</summary>
    extension(FileSelectRoomMapGraphics graphics)
    {
        /// <summary>Captures the room map with its marker and draws it with the game's renderer.</summary>
        internal RgbaPixel[] Render(Position16<MapPixels> scroll, FileSelectStationMarker marker, FileSelectMapAnimations? animations = null) =>
            FrontendScenePixelsAccess.Pixels(graphics.CaptureRenderSnapshot(scroll, marker, animations));

        /// <summary>The room map's fixed BG2 frame alone, as the transition shows it before the map's cells arrive.</summary>
        internal RgbaPixel[] RenderFrameOnly() =>
            FrontendScenePixelsAccess.Pixels(graphics.CaptureRenderSnapshot(content: RoomMapContent.FrameOnly));

        /// <summary>The room map's fixed frame and its map at <paramref name="scroll"/>, without icons.</summary>
        /// <param name="scroll">The map's scroll in pixels.</param>
        internal RgbaPixel[] RenderBackgrounds(Position16<MapPixels> scroll) =>
            FrontendScenePixelsAccess.Pixels(graphics.CaptureRenderSnapshot(scroll));
    }

    /// <summary>The area map's colour math: whether its background is also added to the backdrop.</summary>
    private static AreaMapColorMath ColorMath(bool includeBackdropInColorMath) =>
        includeBackdropInColorMath ? AreaMapColorMath.WithBackdrop : AreaMapColorMath.ForegroundOnly;
}

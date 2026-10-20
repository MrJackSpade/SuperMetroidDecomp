using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access: the file-select map graphics' pixels through their published snapshots.</summary>
internal static class FileSelectMapGraphicsPixelsAccess
{
    /// <summary>The area map's published pixels.</summary>
    extension(FileSelectAreaMapGraphics graphics)
    {
        /// <summary>Captures the area map for the visited-station masks and rasterizes it in software.</summary>
        internal Rgba32[] Render(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true) =>
            FrontendScenePixelsAccess.Pixels(graphics.CaptureRenderSnapshot(usedStationMasks, includeBackdropInColorMath));
    }

    /// <summary>The room map's published pixels.</summary>
    extension(FileSelectRoomMapGraphics graphics)
    {
        /// <summary>Captures the room map with its marker and rasterizes it in software.</summary>
        internal Rgba32[] Render(Position16<MapPixels> scroll, FileSelectStationMarker marker, FileSelectMapAnimations? animations = null) =>
            FrontendScenePixelsAccess.Pixels(graphics.CaptureRenderSnapshot(scroll, marker, animations));
    }
}

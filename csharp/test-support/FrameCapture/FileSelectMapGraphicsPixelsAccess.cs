using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access: the file-select map graphics' pixels through their published snapshots.</summary>
internal static class FileSelectMapGraphicsPixelsAccess
{
    /// <summary>The area map's published pixels.</summary>
    extension(FileSelectAreaMapGraphics graphics)
    {
        /// <summary>
        /// Captures the area map for the visited stations, each area's given as a mask of its station numbers by area
        /// index, and draws it with the game's renderer.
        /// </summary>
        internal RgbaPixel[] Render(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true) =>
            FrontendScenePixelsAccess.Pixels(graphics.Scene(usedStationMasks, includeBackdropInColorMath));

        /// <summary>
        /// The scene <c>Render(usedStationMasks, includeBackdropInColorMath)</c> draws, for a comparison of the pixels two
        /// scenes show (#1532).
        /// </summary>
        internal LayeredScene Scene(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true) =>
            graphics.CaptureRenderSnapshot(UsedStations(usedStationMasks), ColorMath(includeBackdropInColorMath));

        /// <summary>
        /// The area map's BG1 foreground with its additive BG3 background and no labels, drawn with the game's renderer.
        /// </summary>
        /// <param name="includeBackdropInColorMath">True for steady/return $25 color math; false for forward-window $05 behavior, where transparent BG1 cells retain the backdrop without BG3 addition.</param>
        internal RgbaPixel[] RenderBackgrounds(bool includeBackdropInColorMath = true) =>
            FrontendScenePixelsAccess.Pixels(graphics.BackgroundsScene(includeBackdropInColorMath));

        /// <summary>The scene <c>RenderBackgrounds(bool)</c> draws, for a comparison of the pixels two scenes show.</summary>
        internal LayeredScene BackgroundsScene(bool includeBackdropInColorMath = true) =>
            graphics.CaptureBackgrounds(ColorMath(includeBackdropInColorMath));
    }

    /// <summary>The room map's published pixels.</summary>
    extension(FileSelectRoomMapGraphics graphics)
    {
        /// <summary>Captures the room map with its marker and draws it with the game's renderer.</summary>
        internal RgbaPixel[] Render(Position16<MapPixels> scroll, FileSelectStationMarker marker, FileSelectMapAnimations? animations = null) =>
            FrontendScenePixelsAccess.Pixels(graphics.CaptureRenderSnapshot(scroll, marker, animations));

        /// <summary>The room map's fixed BG2 frame alone, as the transition shows it before the map's cells arrive.</summary>
        internal RgbaPixel[] RenderFrameOnly() =>
            FrontendScenePixelsAccess.Pixels(graphics.FrameOnlyScene());

        /// <summary>The scene <see cref="RenderFrameOnly"/> draws, for a comparison of the pixels two scenes show.</summary>
        internal LayeredScene FrameOnlyScene() => graphics.CaptureRenderSnapshot(content: RoomMapContent.FrameOnly);

        /// <summary>The room map's fixed frame and its map at <paramref name="scroll"/>, without icons.</summary>
        /// <param name="scroll">The map's scroll in pixels.</param>
        internal RgbaPixel[] RenderBackgrounds(Position16<MapPixels> scroll) =>
            FrontendScenePixelsAccess.Pixels(graphics.BackgroundsScene(scroll));

        /// <summary>The scene <c>RenderBackgrounds(scroll)</c> draws, for a comparison of the pixels two scenes show.</summary>
        /// <param name="scroll">The map's scroll in pixels.</param>
        internal LayeredScene BackgroundsScene(Position16<MapPixels> scroll) => graphics.CaptureRenderSnapshot(scroll);
    }

    /// <summary>
    /// Progress whose used stations are the stations each area's native used-station word, by area number, records: a
    /// station whose number's bit the word sets; the bits of numbers that name no station Samus uses are never read.
    /// </summary>
    internal static PersistentProgress UsedStations(ReadOnlySpan<ushort> usedStationMasks)
    {
        var progress = new PersistentProgress();
        foreach (LoadStation station in LoadStationDefinitions.Usable)
        {
            int area = SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(station.Area);
            if (area < usedStationMasks.Length && (usedStationMasks[area] & (1 << SuperMetroid.Core.Saves.LoadStationNumberCodec.Number(station))) != 0)
                progress.UsedStations.Add(station);
        }
        return progress;
    }

    /// <summary>Where the area map's background glows: over the whole screen, or only over its foreground.</summary>
    private static AreaMapBackgroundGlow ColorMath(bool includeBackdropInColorMath) =>
        includeBackdropInColorMath ? AreaMapBackgroundGlow.WholeScreen : AreaMapBackgroundGlow.ForegroundOnly;
}

using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The room backgrounds one gameplay frame shows (#1346), resolved from level data with no tilemap: BG1 from the room's
/// level plane, BG2 from its layer-2 plane when that layer streams, the source room's screen during a door's opening
/// scroll, and words a room actor wrote over BG1. A null <see cref="Bg2"/> leaves BG2 to the sky's rows, a boss body
/// that owns BG2 (#1352), or the VRAM tilemap its owner loaded (a library background or the X-ray reveal) until that
/// owner gets its own layer.
/// </summary>
/// <param name="Bg1">The room's level plane on BG1.</param>
/// <param name="Bg2">The room's layer-2 plane on BG2, or null when BG2 shows a loaded tilemap.</param>
/// <param name="Bg3">The HUD and room effect plane on BG3 (#1348).</param>
/// <param name="DoorSource">The source room's BG1 screen during a door's opening scroll, or null.</param>
/// <param name="Bg1Overlay">Words a room actor wrote over BG1, or null.</param>
/// <param name="Sky">The scrolling sky's rows on BG2 when the room has no layer-2 plane, or null (#1347).</param>
/// <param name="Bg2Overlay">Words a room actor wrote over BG2, or null (#1348).</param>
/// <param name="Mode7">The Mode 7 map a Ceres room shows, or null outside Ceres (#1349).</param>
/// <param name="BossBg2">A boss body's tilemap on BG2 while the boss owns BG2, or null (#1352).</param>
public sealed record RoomBackgrounds(RoomBackgroundLayer Bg1, RoomBackgroundLayer? Bg2, Layer3Plane Bg3,
    RoomDoorBackground? DoorSource = null, RoomTilemapOverlay? Bg1Overlay = null, SkyTilemap? Sky = null,
    RoomTilemapOverlay? Bg2Overlay = null, Mode7MapSnapshot? Mode7 = null, BossBg2Tilemap? BossBg2 = null)
{
    /// <summary>The BG1 this frame shows, as a renderer samples it: door source, then overlay, then level data.</summary>
    public IBgTileSource Bg1Source => new Bg1Composite(this);

    /// <summary>
    /// BG2 from a boss body that owns it, the room's layer-2 plane or the sky's rows, or null when BG2 shows its own
    /// loaded tilemap.
    /// </summary>
    public IBgTileSource? Bg2Source => BossBg2 is { } boss ? boss : Bg2 is { } plane ? plane : Sky;

    /// <summary>
    /// BG2 as the frame shows it: <see cref="Bg2Source"/>, or <paramref name="loadedTilemap"/> when BG2 shows its own
    /// tilemap, under any words written over BG2 at their tilemap positions in a <paramref name="mapSize"/> layout.
    /// </summary>
    public IBgTileSource Bg2Shown(IBgTileSource loadedTilemap, Extent16<TilemapCells> mapSize)
    {
        IBgTileSource shown = Bg2Source ?? Ensure.NotNull(loadedTilemap);
        return Bg2Overlay is { } overlay ? new Overlaid(shown, overlay, mapSize) : shown;
    }

    /// <summary>The BG1 tilemap layout BG1 overlays are written in: two screens across.</summary>
    private static Extent16<TilemapCells> Bg1MapSize { get; } = new(2 * SnesPpuLayout.TilemapPageWidthInTiles, SnesPpuLayout.TilemapPageWidthInTiles);

    /// <summary>Words written over a background, then the background.</summary>
    private sealed class Overlaid(IBgTileSource under, RoomTilemapOverlay overlay, Extent16<TilemapCells> mapSize) : IBgTileSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample)
        {
            if (overlay.TryTileAt(scrolled, mapSize, out SnesBgTilemapWord tile))
            {
                sample = BgTileSample.At(tile, scrolled);
                return true;
            }
            return under.TrySample(scrolled, out sample);
        }
    }

    /// <summary>Resolves BG1 in its precedence order.</summary>
    private sealed class Bg1Composite(RoomBackgrounds backgrounds) : IBgTileSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample)
        {
            if (backgrounds.DoorSource is { } door && door.Covers(scrolled))
                return door.SourceRoom.TrySample(scrolled, out sample);
            if (backgrounds.Bg1Overlay is { } overlay && overlay.TryTileAt(scrolled, Bg1MapSize, out SnesBgTilemapWord tile))
            {
                sample = BgTileSample.At(tile, scrolled);
                return true;
            }
            return backgrounds.Bg1.TrySample(scrolled, out sample);
        }
    }
}

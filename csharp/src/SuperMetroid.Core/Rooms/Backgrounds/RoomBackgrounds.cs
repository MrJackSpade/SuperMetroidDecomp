using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The room backgrounds one gameplay frame shows (#1346), resolved from level data with no tilemap: BG1 from the room's
/// level plane, BG2 from its layer-2 plane when that layer streams, the source room's screen during a door's opening
/// scroll, and words a room actor wrote over BG1. A null <see cref="Bg2"/> leaves BG2 to the VRAM tilemap its owner
/// loaded (a library background, the sky, a boss body or the X-ray reveal) until that owner gets its own layer.
/// </summary>
/// <param name="Bg1">The room's level plane on BG1.</param>
/// <param name="Bg2">The room's layer-2 plane on BG2, or null when BG2 shows a loaded tilemap.</param>
/// <param name="DoorSource">The source room's BG1 screen during a door's opening scroll, or null.</param>
/// <param name="Bg1Overlay">Words a room actor wrote over BG1, or null.</param>
/// <param name="Sky">The scrolling sky's rows on BG2 when the room has no layer-2 plane, or null (#1347).</param>
public sealed record RoomBackgrounds(RoomBackgroundLayer Bg1, RoomBackgroundLayer? Bg2,
    RoomDoorBackground? DoorSource = null, RoomTilemapOverlay? Bg1Overlay = null, SkyTilemap? Sky = null)
{
    /// <summary>The BG1 this frame shows, as a renderer samples it: door source, then overlay, then level data.</summary>
    public IBgTileSource Bg1Source => new Bg1Composite(this);

    /// <summary>BG2 from the room's layer-2 plane or the sky's rows, or null when BG2 shows its own loaded tilemap.</summary>
    public IBgTileSource? Bg2Source => Bg2 is { } plane ? plane : Sky;

    /// <summary>Resolves BG1 in its precedence order.</summary>
    private sealed class Bg1Composite(RoomBackgrounds backgrounds) : IBgTileSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample)
        {
            if (backgrounds.DoorSource is { } door && door.Covers(scrolled))
                return door.SourceRoom.TrySample(scrolled, out sample);
            if (backgrounds.Bg1Overlay is { } overlay && overlay.TryTileAt(scrolled, out SnesBgTilemapWord tile))
            {
                sample = BgTileSample.At(tile, scrolled);
                return true;
            }
            return backgrounds.Bg1.TrySample(scrolled, out sample);
        }
    }
}

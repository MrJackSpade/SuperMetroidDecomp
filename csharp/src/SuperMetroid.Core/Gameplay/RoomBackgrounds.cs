using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Gameplay;

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
/// <param name="Characters">Which character page BG1 and BG2 draw from (#1357).</param>
/// <param name="XrayReveal">The X-ray reveal BG2 shows in place of the room's own BG2, or null (#1392).</param>
/// <param name="Bg2Image">The room's library background drawn from its image, where BG2 shows the tilemap it loaded (#1392).</param>
/// <param name="SkyImage">The scrolling sky's rows drawn from their image, where BG2 shows the sky (#1392).</param>
public sealed record RoomBackgrounds(RoomBackgroundLayer Bg1, RoomBackgroundLayer? Bg2, Layer3Plane Bg3,
    RoomDoorBackground? DoorSource = null, RoomTilemapOverlay? Bg1Overlay = null, SkyTilemap? Sky = null,
    RoomTilemapOverlay? Bg2Overlay = null, AffineFloor? Mode7 = null, BossBg2Tilemap? BossBg2 = null,
    RoomBackgroundCharacters Characters = RoomBackgroundCharacters.Tileset, XrayRevealPlane? XrayReveal = null,
    RoomBackgroundImagePlane? Bg2Image = null, RoomBackgroundImagePlane? SkyImage = null)
{
    /// <summary>The BG1 this frame shows, as a renderer samples it: door source, then overlay, then level data.</summary>
    public IBgSource Bg1Source => new Bg1Composite(this);

    /// <summary>
    /// BG2 from a boss body that owns it, the room's layer-2 plane, the X-ray reveal or the sky's rows, or null when BG2
    /// shows its own loaded tilemap.
    /// </summary>
    public IBgSource? Bg2Source => BossBg2 is { } boss ? boss.Shown : Bg2 is { } plane ? plane
        : XrayReveal is { } reveal ? reveal : Sky is { } sky ? SkyImage?.AsSky(sky) ?? new BgTileLayer(sky) : null;

    /// <summary>
    /// BG2 as the frame shows it: <see cref="Bg2Source"/>, or when BG2 shows its own tilemap, the room's library
    /// background image or else <paramref name="loadedTilemap"/>, under any words written over BG2 at their tilemap
    /// positions in a <paramref name="mapSize"/> layout.
    /// </summary>
    public IBgSource Bg2Shown(IBgTileSource loadedTilemap, Extent16<TilemapCells> mapSize)
    {
        IBgSource shown = Bg2Source ?? Bg2Image?.At(mapSize) ?? new BgTileLayer(Ensure.NotNull(loadedTilemap));
        return Bg2Overlay is { } overlay ? new Overlaid(shown, overlay, mapSize) : shown;
    }

    /// <summary>The BG1 tilemap layout BG1 overlays are written in: two screens across.</summary>
    private static Extent16<TilemapCells> Bg1MapSize { get; } = new(2 * SnesPpuLayout.TilemapPageWidthInTiles, SnesPpuLayout.TilemapPageWidthInTiles);

    /// <summary>Words written over a background, then the background.</summary>
    private sealed class Overlaid(IBgSource under, RoomTilemapOverlay overlay, Extent16<TilemapCells> mapSize) : IBgSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
        {
            return overlay.TrySampleAt(scrolled, mapSize, out sample) || under.TrySample(scrolled, out sample);
        }
    }

    /// <summary>Resolves BG1 in its precedence order.</summary>
    private sealed class Bg1Composite(RoomBackgrounds backgrounds) : IBgSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
        {
            if (backgrounds.DoorSource is { } door && door.Covers(scrolled))
                return door.SourceRoom.TrySample(scrolled, out sample);
            if (backgrounds.Bg1Overlay is { } overlay && overlay.TrySampleAt(scrolled, Bg1MapSize, out sample))
                return true;
            return backgrounds.Bg1.TrySample(scrolled, out sample);
        }
    }
}

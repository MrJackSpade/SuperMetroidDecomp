using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room plane shown through a scroll register: the level pixel at a scrolled position is that position less the
/// layer's offset (the BG offset words $091D-$0923), so camera motion, rumble and per-line scroll all move it.
/// </summary>
/// <param name="Plane">The room plane shown.</param>
/// <param name="ScrollOffset">The scroll register less the layer position.</param>
/// <param name="Pictures">
/// The pre-coloured picture each of the plane's blocks shows (#1392), or null for a plane read only for its tile words
/// (the X-ray reveal's copy) or shown in the Ceres Ridley arena, whose planes draw from the object character page until
/// that page's artwork is pictured.
/// </param>
public sealed record RoomBackgroundLayer(RoomLevelPlane Plane, Offset16<BgPlanePixels> ScrollOffset, RoomBlockPictureTable? Pictures)
    : IBgTileSource, IBgSource
{
    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample)
    {
        Position16<BgPlanePixels> level = scrolled - ScrollOffset;
        if (!Plane.TryTileAt(level, out SnesBgTilemapWord tile))
        {
            sample = default;
            return false;
        }
        sample = BgTileSample.At(tile, level);
        return true;
    }

    /// <summary>What the plane shows at a scrolled position: its block's picture pixel, or its tile where it has no pictures.</summary>
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
    {
        if (Pictures is { } pictures)
            return pictures.TrySample(Plane, scrolled - ScrollOffset, out sample);
        bool shown = TrySample(scrolled, out BgTileSample tile);
        sample = BgSample.OfTile(tile);
        return shown;
    }
}

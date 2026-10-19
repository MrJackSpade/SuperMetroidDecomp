namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room plane shown through a scroll register: the level pixel at a scrolled position is that position less the
/// layer's offset (the BG offset words $091D-$0923), so camera motion, rumble and per-line scroll all move it.
/// </summary>
/// <param name="Plane">The room plane shown.</param>
/// <param name="ScrollOffset">The scroll register less the layer position.</param>
public sealed record RoomBackgroundLayer(RoomLevelPlane Plane, Offset16<BgPlanePixels> ScrollOffset) : IBgTileSource
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
}

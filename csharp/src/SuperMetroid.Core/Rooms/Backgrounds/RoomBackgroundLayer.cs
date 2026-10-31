using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room plane shown through a scroll offset: the level pixel at a scrolled position is that position less the
/// layer's offset, so camera motion, rumble and per-line scroll all move it.
/// </summary>
/// <param name="Blocks">The room plane's blocks shown (#1403).</param>
/// <param name="ScrollOffset">The scroll position less the layer position.</param>
/// <param name="Pictures">
/// The pre-coloured picture each of the plane's blocks shows (#1392); null until the frame boundary that latched the plane
/// resolves them.
/// </param>
public sealed record RoomBackgroundLayer(RoomBlockCells Blocks, Offset16<TilePlanePixels> ScrollOffset, RoomBlockPictureTable? Pictures)
{
}

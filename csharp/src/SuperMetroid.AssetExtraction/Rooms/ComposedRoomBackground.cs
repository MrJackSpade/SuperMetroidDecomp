using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// A composed library background image (#1392, #1466): its sheet, its size, and the cells whose pictures change with the
/// room's appearance inputs.
/// </summary>
/// <param name="Sheet">The image's sheet, one frame of what it shows.</param>
/// <param name="Size">The image's width and height, which a part of its picture that shows nothing does not record (#1634).</param>
/// <param name="Cells">The cells the block sheet pictures for their inputs' values.</param>
internal sealed record ComposedRoomBackground(ComposedSpriteSheet Sheet, Extent16<ImagePixels> Size, IReadOnlyList<RoomBackgroundCell> Cells);

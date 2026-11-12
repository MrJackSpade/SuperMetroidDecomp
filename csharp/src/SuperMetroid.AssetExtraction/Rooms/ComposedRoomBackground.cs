using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// A composed library background image (#1392, #1466): its sheet, the tilemap words it pictures, and the cells whose
/// pictures change with the room's appearance inputs.
/// </summary>
/// <param name="Sheet">The image's sheet, one frame of both pages.</param>
/// <param name="Words">The tilemap words pictured, page by page.</param>
/// <param name="Cells">The cells the block sheet pictures for their inputs' values.</param>
internal sealed record ComposedRoomBackground(ComposedSpriteSheet Sheet, ushort[] Words, IReadOnlyList<RoomBackgroundCell> Cells)
{
    /// <summary>Tiles across one row of <see cref="Words"/>: a page, or one period across of a page that repeats (#1675).</summary>
    internal int WidthTiles { get; init; } = RoomBackgroundImageGeometry.PageTiles;
}

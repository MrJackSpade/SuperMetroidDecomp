using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The picture each block definition a room's planes use shows at one moment (#1392): the block over what the room's
/// character bindings show. Immutable; the room resolves a new table when a definition or a binding it reads changes.
/// </summary>
public sealed class RoomBlockPictureTable
{
    /// <summary>The picture of each definition the planes use, null for the rest.</summary>
    private readonly SpriteSheetFrame?[] pictures;

    /// <summary>Keeps the pictures, by definition.</summary>
    internal RoomBlockPictureTable(RoomBlockSheet sheet, SpriteSheetFrame?[] pictures)
    {
        Sheet = Ensure.NotNull(sheet);
        this.pictures = Ensure.NotNull(pictures);
    }

    /// <summary>The tileset's sheet the pictures come from.</summary>
    public RoomBlockSheet Sheet { get; }

    /// <summary>The definitions the table covers, used or not.</summary>
    public int Definitions => pictures.Length;

    /// <summary>The picture definition <paramref name="definition"/> shows, or null when the planes do not use it.</summary>
    public SpriteSheetFrame? Picture(int definition) => pictures[definition];

    /// <summary>The picture pixel <paramref name="blocks"/> show at a level pixel; false outside the room.</summary>
    /// <exception cref="InvalidOperationException">The block there uses a definition the table was not resolved for.</exception>
    public bool TrySample(RoomBlockCells blocks, Position16<BgPlanePixels> level, out BgSample sample)
    {
        Ensure.NotNull(blocks);
        if (!blocks.TryCellAt(level, out RoomBlockCell cell, out Position16<BgPlanePixels> within))
        {
            sample = default;
            return false;
        }
        SpriteSheetFrame picture = pictures[cell.Definition]
            ?? throw new InvalidOperationException($"Block definition {cell.Definition} was not resolved for the plane showing it.");
        LevelBlockFlipFlags flips = cell.Flips;
        int x = within.X, y = within.Y;
        sample = BgSample.OfPicture(picture, new Position16<ImagePixels>(
            (ushort)((flips & LevelBlockFlipFlags.Horizontal) != 0 ? RoomLevelPlane.BlockPixels - 1 - x : x),
            (ushort)((flips & LevelBlockFlipFlags.Vertical) != 0 ? RoomLevelPlane.BlockPixels - 1 - y : y)));
        return true;
    }
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The picture each block a room's planes use shows at one moment (#1392, #1431): the block over what the room's character
/// bindings show. Immutable; the room resolves a new table when a block it shows or a binding it reads changes.
/// </summary>
public sealed class RoomBlockPictureTable
{
    /// <summary>The picture of each block the planes use, null for the rest.</summary>
    private readonly SpriteSheetFrame?[] pictures;

    /// <summary>Keeps the pictures, by block.</summary>
    internal RoomBlockPictureTable(RoomBlockSheet sheet, SpriteSheetFrame?[] pictures)
    {
        Sheet = Ensure.NotNull(sheet);
        this.pictures = Ensure.NotNull(pictures);
    }

    /// <summary>The tileset's sheet the pictures come from.</summary>
    public RoomBlockSheet Sheet { get; }

    /// <summary>The blocks the table covers, used or not.</summary>
    public int Blocks => pictures.Length;

    /// <summary>The picture block <paramref name="block"/> shows, or null when the planes do not use it.</summary>
    public SpriteSheetFrame? Picture(int block) => pictures[block];
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The picture each block a room's planes use shows at one moment (#1392, #1431, #1466): the block for the values its
/// appearance inputs take, as its part behind the actors passing in front of the room's back tiles and its part in front
/// of them (#1634). Immutable; the room resolves a new table when a block it shows or an input it reads changes.
/// </summary>
public sealed class RoomBlockPictureTable
{
    /// <summary>The picture of each block the planes use, null for the rest.</summary>
    private readonly LayeredPicture?[] pictures;

    /// <summary>Keeps the pictures, by block.</summary>
    internal RoomBlockPictureTable(LayeredPicture?[] pictures) => this.pictures = Ensure.NotNull(pictures);

    /// <summary>
    /// The table of a room whose frame draws none of its level's blocks: the Ceres elevator shaft, whose screen is its turning
    /// floor (#1647). No block has a picture.
    /// </summary>
    internal static RoomBlockPictureTable NoneDrawn() => new(new LayeredPicture?[CollectibleCellPictures.End]);

    /// <summary>The blocks the table covers, used or not.</summary>
    public int Blocks => pictures.Length;

    /// <summary>
    /// The picture block <paramref name="block"/> shows, or null when the planes do not use it; a used block whose picture
    /// shows nothing has neither part.
    /// </summary>
    public LayeredPicture? Picture(int block) => pictures[block];
}

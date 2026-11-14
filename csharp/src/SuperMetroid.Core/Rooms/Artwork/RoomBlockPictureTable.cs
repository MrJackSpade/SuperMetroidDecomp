using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The picture each block and collectible picture a room's planes use shows at one moment (#1392, #1431, #1466, #1764): the
/// block for the values its appearance inputs take, and the collectible's own picture (#1538), as its part behind the actors
/// passing in front of the room's back tiles and its part in front of them (#1634). Immutable; the room resolves a new table
/// when a picture it shows or an input it reads changes.
/// </summary>
public sealed class RoomBlockPictureTable
{
    /// <summary>The picture of each block the planes use, by block, null for the rest.</summary>
    private readonly LayeredPicture?[] blocks;

    /// <summary>The picture each collectible picture the planes use shows.</summary>
    private readonly Dictionary<CollectiblePicture, LayeredPicture> collectibles;

    /// <summary>Keeps the blocks' pictures, by block, and the collectible pictures'.</summary>
    internal RoomBlockPictureTable(LayeredPicture?[] blocks, IReadOnlyDictionary<CollectiblePicture, LayeredPicture> collectibles)
    {
        this.blocks = Ensure.NotNull(blocks);
        this.collectibles = new Dictionary<CollectiblePicture, LayeredPicture>(Ensure.NotNull(collectibles));
    }

    /// <summary>
    /// The table of a room whose frame draws none of its level's blocks: the Ceres elevator shaft, whose screen is its turning
    /// floor (#1647). No block and no collectible has a picture.
    /// </summary>
    internal static RoomBlockPictureTable NoneDrawn() =>
        new(new LayeredPicture?[RoomBlockAppearance.LastBlock + 1], new Dictionary<CollectiblePicture, LayeredPicture>());

    /// <summary>The blocks the table covers, used or not: blocks zero up to this.</summary>
    public int Blocks => blocks.Length;

    /// <summary>The picture each collectible picture the planes use shows.</summary>
    internal IReadOnlyDictionary<CollectiblePicture, LayeredPicture> Collectibles => collectibles;

    /// <summary>
    /// The picture <paramref name="shown"/> shows, or null when the planes do not use it; a used block whose picture shows
    /// nothing has neither part.
    /// </summary>
    public LayeredPicture? Picture(RoomCellPicture shown) =>
        shown.ShowsCollectible ? collectibles.GetValueOrDefault(shown.Collectible) : blocks[shown.Block];
}

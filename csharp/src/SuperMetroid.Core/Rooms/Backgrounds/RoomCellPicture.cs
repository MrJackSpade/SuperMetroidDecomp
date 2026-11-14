using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What one cell of a room plane or of the X-ray reveal shows (#1764): a block of the room tileset's block set, or the
/// picture of a collectible that shows its own picture, by its kind and animation frame (#1538). A collectible's picture
/// is named by what it shows, not numbered past the tileset's blocks. The default shows the block set's first block.
/// </summary>
public readonly record struct RoomCellPicture
{
    /// <summary>The block shown, by its number in the tileset's block set; zero where a collectible's picture shows.</summary>
    private readonly int block;

    /// <summary>The collectible picture shown, or null where a block shows.</summary>
    private readonly CollectiblePicture? collectible;

    /// <summary>Keeps a block or a collectible picture, never both.</summary>
    private RoomCellPicture(int block, CollectiblePicture? collectible)
    {
        this.block = block;
        this.collectible = collectible;
    }

    /// <summary>Whether the cell shows a collectible's picture rather than a block of the tileset.</summary>
    public bool ShowsCollectible => collectible is not null;

    /// <summary>The block shown, by its number in the room tileset's block set.</summary>
    /// <exception cref="InvalidOperationException">The cell shows a collectible's picture.</exception>
    public int Block => collectible is null ? block
        : throw new InvalidOperationException($"The cell shows collectible {collectible.Value.Kind}'s picture, not a block.");

    /// <summary>The collectible picture shown.</summary>
    /// <exception cref="InvalidOperationException">The cell shows a block of the tileset.</exception>
    public CollectiblePicture Collectible => collectible
        ?? throw new InvalidOperationException($"The cell shows block {block}, not a collectible's picture.");

    /// <summary>A cell showing block <paramref name="block"/> of the room tileset's block set.</summary>
    /// <param name="block">The block's number in the tileset's block set, 0 through <see cref="RoomBlockAppearance.LastBlock"/>.</param>
    public static RoomCellPicture OfBlock(int block) =>
        new(Ensure.BetweenInclusive(0, RoomBlockAppearance.LastBlock, block), null);

    /// <summary>A cell showing the collectible picture <paramref name="picture"/>.</summary>
    public static RoomCellPicture Of(CollectiblePicture picture) => new(0, picture);

    /// <summary>The block or the collectible picture shown, for diagnostics.</summary>
    public override string ToString() =>
        collectible is { } shown ? $"collectible {shown.Kind} frame {shown.Frame}" : $"block {block}";
}

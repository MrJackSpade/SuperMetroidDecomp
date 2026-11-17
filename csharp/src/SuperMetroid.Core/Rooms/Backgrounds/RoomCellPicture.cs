using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What one cell of a room plane or of the X-ray reveal shows (#1764, #2100): a block of the room tileset's block set, the
/// picture of a collectible that shows its own picture, by its kind and animation frame (#1538), or nothing. A
/// collectible's picture is named by what it shows, not numbered past the tileset's blocks, and nothing is no block at
/// all. The default shows the block set's first block.
/// </summary>
public readonly record struct RoomCellPicture
{
    /// <summary>The block shown, by its number in the tileset's block set; zero where no block shows.</summary>
    private readonly int block;

    /// <summary>The collectible picture shown, or null where a block or nothing shows.</summary>
    private readonly CollectiblePicture? collectible;

    /// <summary>Whether the cell shows nothing at all.</summary>
    private readonly Blank blank;

    /// <summary>Whether a cell that shows no collectible's picture shows a block.</summary>
    private enum Blank
    {
        /// <summary>A block or a collectible's picture shows.</summary>
        No,
        /// <summary>Nothing shows.</summary>
        ShowsNothing,
    }

    /// <summary>Keeps a block, a collectible picture or nothing, never more than one.</summary>
    private RoomCellPicture(int block, CollectiblePicture? collectible, Blank blank)
    {
        this.block = block;
        this.collectible = collectible;
        this.blank = blank;
    }

    /// <summary>A cell that shows nothing.</summary>
    public static RoomCellPicture Nothing { get; } = new(0, null, Blank.ShowsNothing);

    /// <summary>Whether the cell shows a collectible's picture rather than a block of the tileset.</summary>
    public bool ShowsCollectible => collectible is not null;

    /// <summary>Whether the cell shows nothing.</summary>
    public bool ShowsNothing => blank == Blank.ShowsNothing;

    /// <summary>The block shown, by its number in the room tileset's block set.</summary>
    /// <exception cref="InvalidOperationException">The cell shows a collectible's picture or nothing.</exception>
    public int Block => collectible is null && !ShowsNothing ? block
        : throw new InvalidOperationException($"The cell shows {this}, not a block.");

    /// <summary>The collectible picture shown.</summary>
    /// <exception cref="InvalidOperationException">The cell shows a block of the tileset or nothing.</exception>
    public CollectiblePicture Collectible => collectible
        ?? throw new InvalidOperationException($"The cell shows {this}, not a collectible's picture.");

    /// <summary>A cell showing block <paramref name="block"/> of the room tileset's block set.</summary>
    /// <param name="block">The block's number in the tileset's block set, zero or more; the tileset's sheet bounds it.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block is negative.</exception>
    public static RoomCellPicture OfBlock(int block) => new(Ensure.AtLeastZero(block), null, Blank.No);

    /// <summary>A cell showing what <paramref name="appearance"/> shows: its block, or nothing.</summary>
    /// <exception cref="InvalidOperationException">
    /// The appearance is terrain beyond the room, which no cell shows, or a collectible's pair block, which its collectible's
    /// own picture covers wherever it lies (#2354).
    /// </exception>
    public static RoomCellPicture Of(RoomBlockAppearance appearance) =>
        appearance.ShowsNothing ? Nothing
            : appearance.ShowsBlock ? OfBlock(appearance.Block)
            : appearance.IsCollectiblePairBlock
                ? throw new InvalidOperationException($"No room cell shows {appearance}: its collectible's own picture covers it.")
            : throw new InvalidOperationException($"No room cell shows {appearance}: the room never shows terrain beyond its grid.");

    /// <summary>A cell showing the collectible picture <paramref name="picture"/>.</summary>
    public static RoomCellPicture Of(CollectiblePicture picture) => new(0, picture, Blank.No);

    /// <summary>The block, the collectible picture or nothing shown, for diagnostics.</summary>
    public override string ToString() =>
        collectible is { } shown ? $"collectible {shown.Kind} {shown.Look}" : ShowsNothing ? "nothing" : $"block {block}";
}

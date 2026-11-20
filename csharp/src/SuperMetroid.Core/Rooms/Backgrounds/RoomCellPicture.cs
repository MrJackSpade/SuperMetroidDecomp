using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What one cell of a room plane or of the X-ray reveal shows (#1764, #2100): a block of the room tileset's block set, the
/// picture of a collectible that shows its own picture, by its kind and animation frame (#1538), or nothing. A
/// collectible's picture is named by what it shows, not numbered past the tileset's blocks, and nothing is no block at
/// all. A cell carries only what it shows (#2558): no block number where no block shows, and the default shows nothing
/// of the three.
/// </summary>
public readonly record struct RoomCellPicture
{
    /// <summary>What the cell shows: a block, a collectible's picture or nothing.</summary>
    private readonly Shown shown;

    /// <summary>What a cell shows, each kind carrying only its own values.</summary>
    private abstract record Shown;

    /// <summary>A block of the room tileset's block set.</summary>
    /// <param name="Block">The block's number in the tileset's block set.</param>
    private sealed record ShownBlock(int Block) : Shown;

    /// <summary>A collectible's own picture.</summary>
    /// <param name="Picture">The picture shown.</param>
    private sealed record ShownCollectible(CollectiblePicture Picture) : Shown;

    /// <summary>Nothing at all.</summary>
    private sealed record ShownNothing : Shown;

    /// <summary>Keeps what the cell shows.</summary>
    private RoomCellPicture(Shown shown) => this.shown = shown;

    /// <summary>A cell that shows nothing.</summary>
    public static RoomCellPicture Nothing { get; } = new(new ShownNothing());

    /// <summary>Whether the cell shows a collectible's picture rather than a block of the tileset.</summary>
    public bool ShowsCollectible => shown is ShownCollectible;

    /// <summary>Whether the cell shows nothing.</summary>
    public bool ShowsNothing => shown is ShownNothing;

    /// <summary>The block shown, by its number in the room tileset's block set.</summary>
    /// <exception cref="InvalidOperationException">The cell shows a collectible's picture or nothing.</exception>
    public int Block => shown is ShownBlock block ? block.Block
        : throw new InvalidOperationException($"The cell shows {this}, not a block.");

    /// <summary>The collectible picture shown, or none where the cell shows a block of the tileset or nothing.</summary>
    public CollectiblePicture? Collectible => collectible;

    /// <summary>A cell showing block <paramref name="block"/> of the room tileset's block set.</summary>
    /// <param name="block">The block's number in the tileset's block set, zero or more; the tileset's sheet bounds it.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block is negative.</exception>
    public static RoomCellPicture OfBlock(int block) => new(new ShownBlock(Ensure.AtLeastZero(block)));

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
    public static RoomCellPicture Of(CollectiblePicture picture) => new(new ShownCollectible(picture));

    /// <summary>The block, the collectible picture or nothing shown, for diagnostics.</summary>
    public override string ToString() => shown switch
    {
        ShownCollectible collectible => $"collectible {collectible.Picture.Kind} {collectible.Picture.Look}",
        ShownNothing => "nothing",
        ShownBlock block => $"block {block.Block}",
        _ => "no picture",
    };
}

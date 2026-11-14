using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves the picture each block and collectible picture a room's planes use shows (#1392, #1431, #1466, #1764): a
/// block, by its index, for the values the appearance inputs the sheet records it depends on take now, and a collectible's
/// own picture (#1538). A block is resolved again only when one of its inputs changes, so a frame whose inputs stand still
/// reuses its table.
/// </summary>
/// <param name="sheet">The room tileset's block sheet.</param>
public sealed class RoomBlockPictureResolver(RoomBlockSheet sheet)
{
    /// <summary>The foreground plane whose used pictures <see cref="used"/> holds.</summary>
    private RoomLevelPlane? foregroundPlane;

    /// <summary>The background plane whose used pictures <see cref="used"/> holds, or null.</summary>
    private RoomLevelPlane? backgroundPlane;

    /// <summary>The further pictures <see cref="used"/> holds.</summary>
    private HashSet<RoomCellPicture> further = [];

    /// <summary>The blocks and collectible pictures the planes use.</summary>
    private RoomCellPicture[] used = [];

    /// <summary>The value each input of each used picture took when it was last resolved, by used picture.</summary>
    private string[][] values = [];

    /// <summary>The last table resolved.</summary>
    private RoomBlockPictureTable? table;

    /// <summary>The room tileset's block sheet.</summary>
    public RoomBlockSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>
    /// The pictures the blocks and collectibles of <paramref name="foreground"/> and <paramref name="background"/> show while
    /// the room's appearance inputs take the values <paramref name="appearance"/> reads.
    /// </summary>
    /// <param name="foreground">The room's foreground plane.</param>
    /// <param name="background">The room's background plane, which shows the same tileset's blocks, or null when background shows none.</param>
    /// <param name="appearance">The values the room's appearance inputs take now.</param>
    /// <param name="alsoShown">Further blocks and collectible pictures the frame shows, as the X-ray reveal's; none when empty.</param>
    /// <exception cref="InvalidDataException">
    /// A block is not among those the tileset loads, or the sheet holds no picture of a block, or none for its inputs' values,
    /// naming the sheet and the block (#1636).
    /// </exception>
    public RoomBlockPictureTable Resolve(RoomLevelPlane foreground, RoomLevelPlane? background, RoomAppearanceView appearance,
        IEnumerable<RoomCellPicture> alsoShown)
    {
        Ensure.NotNull(foreground);
        HashSet<RoomCellPicture> also = [.. Ensure.NotNull(alsoShown)];
        RoomBlockPictureTable? previous = table;
        bool fresh = previous is null || !ReferenceEquals(foregroundPlane, foreground) || !ReferenceEquals(backgroundPlane, background)
            || !also.SetEquals(further);
        if (fresh)
        {
            foregroundPlane = foreground;
            backgroundPlane = background;
            further = also;
            used = [.. Uses(foreground).Concat(background is null ? [] : Uses(background)).Concat(also).Distinct()];
            values = [.. used.Select(shown => new string[InputsOf(shown).Length])];
        }
        LayeredPicture?[]? blocks = fresh ? new LayeredPicture?[RoomBlockAppearance.LastBlock + 1] : null;
        Dictionary<CollectiblePicture, LayeredPicture>? collectibles = fresh ? [] : null;
        for (int index = 0; index < used.Length; index++)
        {
            RoomCellPicture shown = used[index];
            ImmutableArray<AppearanceInput> inputs = InputsOf(shown);
            string[] shownValues = values[index];
            bool changed = fresh;
            for (int input = 0; input < inputs.Length; input++)
            {
                string now = appearance.ValueOf(inputs[input]);
                changed |= now != shownValues[input];
                shownValues[input] = now;
            }
            if (!changed)
                continue;
            if (blocks is null || collectibles is null)
            {
                RoomBlockPictureTable changing = previous ?? throw new InvalidOperationException("A changed block was resolved with no table to change.");
                blocks = Copy(changing);
                collectibles = new Dictionary<CollectiblePicture, LayeredPicture>(changing.Collectibles);
            }
            LayeredPicture picture = PictureOf(shown, inputs, shownValues);
            if (shown.ShowsCollectible)
                collectibles[shown.Collectible] = picture;
            else
                blocks[shown.Block] = picture;
        }
        RoomBlockPictureTable resolved = blocks is null || collectibles is null
            ? previous ?? throw new InvalidOperationException("An unchanged resolution has no table.")
            : new RoomBlockPictureTable(blocks, collectibles);
        table = resolved;
        return resolved;
    }

    /// <summary>The blocks and collectible pictures a plane's cells show.</summary>
    private static HashSet<RoomCellPicture> Uses(RoomLevelPlane plane)
    {
        var uses = new HashSet<RoomCellPicture>();
        foreach (RoomBlockCell cell in plane.Cells.Cells)
            uses.Add(cell.Picture);
        return uses;
    }

    /// <summary>The inputs the picture <paramref name="shown"/> depends on: none for a collectible's picture.</summary>
    /// <exception cref="InvalidDataException">The sheet has no picture of the block (#1636).</exception>
    private ImmutableArray<AppearanceInput> InputsOf(RoomCellPicture shown) =>
        shown.ShowsCollectible ? [] : Sheet.InputsOf(shown.Block);

    /// <summary>
    /// The picture <paramref name="shown"/> shows while its inputs take <paramref name="shownValues"/>: a collectible's is its
    /// kind's alone, which nothing the room shows changes (#1538).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet has no picture of the block for those values, or of the collectible (#1636).</exception>
    private LayeredPicture PictureOf(RoomCellPicture shown, ImmutableArray<AppearanceInput> inputs, string[] shownValues) =>
        shown.ShowsCollectible ? Sheet.PictureOf(shown.Collectible) : Sheet.BlockPicture(shown.Block, inputs, shownValues);

    /// <summary>A copy of a table's block pictures, for a table that changes in some.</summary>
    private static LayeredPicture?[] Copy(RoomBlockPictureTable previous)
    {
        var pictures = new LayeredPicture?[previous.Blocks];
        for (int block = 0; block < pictures.Length; block++)
            pictures[block] = previous.Picture(RoomCellPicture.OfBlock(block));
        return pictures;
    }
}

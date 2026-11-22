using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves the picture each block and collectible picture a room's planes use shows (#1392, #1431, #1466, #1764): a
/// block, by its index among the tileset's installed blocks (#2100), for the values the appearance inputs the sheet records
/// it depends on take now, and a collectible's own picture (#1538); a cell that shows nothing needs no picture. A block is
/// resolved again only when one of its inputs changes, so a frame whose inputs stand still reuses its table.
/// </summary>
/// <param name="sheet">The room tileset's block sheet.</param>
public sealed partial class RoomBlockPictureResolver(RoomBlockSheet sheet)
{
    /// <summary>The foreground plane whose used pictures <see cref="used"/> holds.</summary>
    private RoomLevelPlane? foregroundPlane;

    /// <summary>The background plane whose used pictures <see cref="used"/> holds, or null.</summary>
    private RoomLevelPlane? backgroundPlane;

    /// <summary>The further pictures <see cref="used"/> holds.</summary>
    private HashSet<RoomCellPicture> further = [];

    /// <summary>The blocks and collectible pictures the planes use, each with the value each of its inputs took when it was last resolved.</summary>
    private UsedPicture[] used = [];

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
            used = [.. Uses(foreground).Concat(background is null ? [] : Uses(background))
                .Concat(also.Where(shown => !shown.ShowsNothing)).Distinct()
                .Select(shown => new UsedPicture(shown, new string[InputsOf(shown).Length]))];
        }
        RoomBlockPictureTable resolved = !fresh && previous is { } unchanged ? Update(unchanged, appearance) : Build(appearance);
        table = resolved;
        return resolved;
    }

    /// <summary>Resolves every used picture afresh for the values <paramref name="appearance"/> reads.</summary>
    private RoomBlockPictureTable Build(RoomAppearanceView appearance)
    {
        var blocks = new LayeredPicture?[Sheet.Inputs.Count];
        var collectibles = new Dictionary<CollectiblePicture, LayeredPicture>();
        foreach (UsedPicture entry in used)
        {
            RoomCellPicture shown = entry.Picture;
            ImmutableArray<AppearanceInput> inputs = InputsOf(shown);
            TakeValues(appearance, inputs, entry.Values);
            Place(blocks, collectibles, shown, PictureOf(shown, inputs, entry.Values));
        }
        return new RoomBlockPictureTable(blocks, collectibles);
    }

    /// <summary>
    /// Resolves again only the used pictures whose inputs <paramref name="appearance"/> reads have changed since
    /// <paramref name="previous"/> was resolved; with none changed, <paramref name="previous"/> stands.
    /// </summary>
    private RoomBlockPictureTable Update(RoomBlockPictureTable previous, RoomAppearanceView appearance)
    {
        LayeredPicture?[]? blocks = null;
        Dictionary<CollectiblePicture, LayeredPicture>? collectibles = null;
        foreach (UsedPicture entry in used)
        {
            RoomCellPicture shown = entry.Picture;
            ImmutableArray<AppearanceInput> inputs = InputsOf(shown);
            if (!TakeValues(appearance, inputs, entry.Values))
                continue;
            blocks ??= Copy(previous);
            collectibles ??= new Dictionary<CollectiblePicture, LayeredPicture>(previous.Collectibles);
            Place(blocks, collectibles, shown, PictureOf(shown, inputs, entry.Values));
        }
        return blocks is null || collectibles is null ? previous : new RoomBlockPictureTable(blocks, collectibles);
    }

    /// <summary>Takes the values <paramref name="appearance"/> reads for <paramref name="inputs"/>, and whether any changed.</summary>
    private static bool TakeValues(RoomAppearanceView appearance, ImmutableArray<AppearanceInput> inputs, string[] shownValues)
    {
        bool changed = false;
        for (int input = 0; input < inputs.Length; input++)
        {
            string now = appearance.ValueOf(inputs[input]);
            changed |= now != shownValues[input];
            shownValues[input] = now;
        }
        return changed;
    }

    /// <summary>Places <paramref name="picture"/> as the picture <paramref name="shown"/> shows.</summary>
    private static void Place(LayeredPicture?[] blocks, Dictionary<CollectiblePicture, LayeredPicture> collectibles,
        RoomCellPicture shown, LayeredPicture picture)
    {
        if (shown.ShowsCollectible)
            collectibles[shown.Collectible] = picture;
        else
            blocks[shown.Block] = picture;
    }

    /// <summary>The blocks and collectible pictures a plane's cells show; a cell that shows nothing uses neither.</summary>
    private static HashSet<RoomCellPicture> Uses(RoomLevelPlane plane)
    {
        var uses = new HashSet<RoomCellPicture>();
        foreach (RoomBlockCell cell in plane.Cells.Cells)
        {
            if (!cell.Picture.ShowsNothing)
                uses.Add(cell.Picture);
        }
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

    /// <summary>One picture the planes use, with the value each of its inputs took when it was last resolved.</summary>
    /// <param name="Picture">The block or collectible picture.</param>
    /// <param name="Values">The value each of its inputs took when last resolved, in the order of its inputs.</param>
    private sealed partial record UsedPicture(RoomCellPicture Picture, string[] Values);
}

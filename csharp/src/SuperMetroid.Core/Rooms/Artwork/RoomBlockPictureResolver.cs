using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves the picture each block a room's planes use shows (#1392, #1431, #1466): the block, by its index, for the
/// values the appearance inputs the sheet records it depends on take now. A block is resolved again only when one of its
/// inputs changes, so a frame whose inputs stand still reuses its table.
/// </summary>
/// <param name="sheet">The room tileset's block sheet.</param>
public sealed class RoomBlockPictureResolver(RoomBlockSheet sheet)
{
    /// <summary>The foreground plane whose used blocks <see cref="used"/> holds.</summary>
    private RoomLevelPlane? foregroundPlane;

    /// <summary>The background plane whose used blocks <see cref="used"/> holds, or null.</summary>
    private RoomLevelPlane? backgroundPlane;

    /// <summary>The artwork the blocks in <see cref="used"/> were resolved over.</summary>
    private RoomBlockArtwork artwork;

    /// <summary>The further blocks <see cref="used"/> holds, ascending.</summary>
    private int[] further = [];

    /// <summary>The blocks the planes use, ascending.</summary>
    private int[] used = [];

    /// <summary>The value each input of each used block took when it was last resolved, by used block.</summary>
    private string[][] values = [];

    /// <summary>The last table resolved.</summary>
    private RoomBlockPictureTable? table;

    /// <summary>The room tileset's block sheet.</summary>
    public RoomBlockSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>
    /// The pictures the blocks of <paramref name="foreground"/> and <paramref name="background"/> show over <paramref name="blockArtwork"/>
    /// while the room's appearance inputs take the values <paramref name="appearance"/> reads.
    /// </summary>
    /// <param name="foreground">The room's foreground plane.</param>
    /// <param name="background">The room's background plane, which shows the same tileset's blocks, or null when background shows none.</param>
    /// <param name="blockArtwork">The artwork the room's blocks show.</param>
    /// <param name="appearance">The values the room's appearance inputs take now.</param>
    /// <param name="alsoShown">Further blocks the frame shows, as the X-ray reveal's blocks; none when empty.</param>
    /// <exception cref="InvalidDataException">
    /// A block is not among those the tileset loads, or the sheet holds no picture of a block, or none for its inputs' values,
    /// naming the sheet and the block (#1636).
    /// </exception>
    public RoomBlockPictureTable Resolve(RoomLevelPlane foreground, RoomLevelPlane? background, RoomBlockArtwork blockArtwork, RoomAppearanceView appearance,
        IEnumerable<int> alsoShown)
    {
        Ensure.NotNull(foreground);
        int[] also = [.. Ensure.NotNull(alsoShown).Distinct().Order()];
        RoomBlockPictureTable? previous = table;
        bool fresh = previous is null || !ReferenceEquals(foregroundPlane, foreground) || !ReferenceEquals(backgroundPlane, background) || artwork != blockArtwork
            || !also.SequenceEqual(further);
        if (fresh)
        {
            foregroundPlane = foreground;
            backgroundPlane = background;
            artwork = blockArtwork;
            further = also;
            used = [.. Uses(foreground).Concat(background is null ? [] : Uses(background)).Concat(also).Distinct().Order()];
            values = [.. used.Select(block => new string[InputsOf(block, blockArtwork).Length])];
        }
        LayeredPicture?[]? pictures = fresh ? new LayeredPicture?[CollectibleCellPictures.End] : null;
        for (int index = 0; index < used.Length; index++)
        {
            int block = used[index];
            ImmutableArray<AppearanceInput> inputs = InputsOf(block, blockArtwork);
            string[] blockValues = values[index];
            bool changed = fresh;
            for (int input = 0; input < inputs.Length; input++)
            {
                string now = appearance.ValueOf(inputs[input]);
                changed |= now != blockValues[input];
                blockValues[input] = now;
            }
            if (!changed)
                continue;
            pictures ??= Copy(previous ?? throw new InvalidOperationException("A changed block was resolved with no table to change."));
            pictures[block] = PictureOf(block, blockArtwork, inputs, blockValues);
        }
        RoomBlockPictureTable resolved = pictures is null
            ? previous ?? throw new InvalidOperationException("An unchanged resolution has no table.")
            : new RoomBlockPictureTable(pictures);
        table = resolved;
        return resolved;
    }

    /// <summary>The blocks and collectible pictures a plane's cells show.</summary>
    private static HashSet<int> Uses(RoomLevelPlane plane)
    {
        var uses = new HashSet<int>();
        foreach (RoomBlockCell cell in plane.Cells.Cells)
            uses.Add(cell.Definition);
        return uses;
    }

    /// <summary>The inputs the picture <paramref name="definition"/> names depends on: none for a collectible's picture.</summary>
    /// <exception cref="InvalidDataException">The sheet has no picture of the block (#1636).</exception>
    private ImmutableArray<AppearanceInput> InputsOf(int definition, RoomBlockArtwork blockArtwork) =>
        CollectibleCellPictures.TryPictureOf(definition, out _) ? [] : Sheet.InputsOf(definition, blockArtwork);

    /// <summary>
    /// The picture <paramref name="definition"/> shows over <paramref name="blockArtwork"/> while its inputs take
    /// <paramref name="blockValues"/>: a collectible's is its kind's alone, which nothing the room shows changes (#1538).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The sheet has no picture of the block for those values (#1636), or a collectible shows in a room whose blocks show the
    /// sprites' artwork, where none lies.
    /// </exception>
    private LayeredPicture PictureOf(int definition, RoomBlockArtwork blockArtwork, ImmutableArray<AppearanceInput> inputs, string[] blockValues)
    {
        if (!CollectibleCellPictures.TryPictureOf(definition, out CollectiblePicture collectible))
            return Sheet.BlockPicture(definition, blockArtwork, inputs, blockValues);
        return blockArtwork == RoomBlockArtwork.Tileset ? Sheet.PictureOf(collectible)
            : throw new InvalidDataException($"A room showing its blocks over {blockArtwork} shows collectible {collectible.Kind}.");
    }

    /// <summary>A copy of a table's pictures, for a table that changes in some.</summary>
    private static LayeredPicture?[] Copy(RoomBlockPictureTable previous)
    {
        var pictures = new LayeredPicture?[previous.Blocks];
        for (int block = 0; block < pictures.Length; block++)
            pictures[block] = previous.Picture(block);
        return pictures;
    }
}

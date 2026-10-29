using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves the picture each block a room's planes use shows (#1392, #1431, #1466): the block, by its index, for the
/// values the appearance inputs the sheet records it depends on take now. A block is resolved again only when one of its
/// inputs changes, so a frame whose inputs stand still reuses its table.
/// </summary>
/// <param name="sheet">The room tileset's block sheet.</param>
public sealed class RoomBlockPictureResolver(RoomBlockSheet sheet)
{
    /// <summary>The BG1 plane whose used blocks <see cref="used"/> holds.</summary>
    private RoomLevelPlane? bg1Plane;

    /// <summary>The layer-2 plane whose used blocks <see cref="used"/> holds, or null.</summary>
    private RoomLevelPlane? bg2Plane;

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
    /// The pictures the blocks of <paramref name="bg1"/> and <paramref name="bg2"/> show over <paramref name="blockArtwork"/>
    /// while the room's appearance inputs take the values <paramref name="appearance"/> reads.
    /// </summary>
    /// <param name="bg1">The room's BG1 plane.</param>
    /// <param name="bg2">The room's layer-2 plane, which shows the same tileset's blocks, or null when BG2 shows none.</param>
    /// <param name="blockArtwork">The artwork the room's blocks show.</param>
    /// <param name="appearance">The values the room's appearance inputs take now.</param>
    /// <param name="alsoShown">Further blocks the frame shows, as the X-ray reveal's blocks; none when empty.</param>
    /// <exception cref="InvalidDataException">
    /// A block is not among those the tileset loads, or the sheet holds no picture of a block for its inputs' values.
    /// </exception>
    public RoomBlockPictureTable Resolve(RoomLevelPlane bg1, RoomLevelPlane? bg2, RoomBlockArtwork blockArtwork, RoomAppearanceView appearance,
        IEnumerable<int> alsoShown)
    {
        Ensure.NotNull(bg1);
        int[] also = [.. Ensure.NotNull(alsoShown).Distinct().Order()];
        RoomBlockPictureTable? previous = table;
        bool fresh = previous is null || !ReferenceEquals(bg1Plane, bg1) || !ReferenceEquals(bg2Plane, bg2) || artwork != blockArtwork
            || !also.SequenceEqual(further);
        if (fresh)
        {
            bg1Plane = bg1;
            bg2Plane = bg2;
            artwork = blockArtwork;
            further = also;
            used = [.. Uses(bg1).Concat(bg2 is null ? [] : Uses(bg2)).Concat(also).Distinct().Order()];
            values = [.. used.Select(block => new string[Sheet.Inputs.Of(block, blockArtwork).Length])];
        }
        SpriteSheetFrame?[]? pictures = fresh ? new SpriteSheetFrame?[Sheet.Inputs.Count] : null;
        for (int index = 0; index < used.Length; index++)
        {
            int block = used[index];
            ImmutableArray<AppearanceInput> inputs = Sheet.Inputs.Of(block, blockArtwork);
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
            pictures[block] = Sheet.Picture(RoomBlockPictureNames.Of(block, blockArtwork, inputs, blockValues));
        }
        RoomBlockPictureTable resolved = pictures is null
            ? previous ?? throw new InvalidOperationException("An unchanged resolution has no table.")
            : new RoomBlockPictureTable(Sheet, pictures);
        table = resolved;
        return resolved;
    }

    /// <summary>The blocks a plane uses.</summary>
    private static HashSet<int> Uses(RoomLevelPlane plane)
    {
        var uses = new HashSet<int>();
        foreach (RoomLevelWord block in plane.Blocks)
            uses.Add(block.VisualBlockIndex);
        return uses;
    }

    /// <summary>A copy of a table's pictures, for a table that changes in some.</summary>
    private static SpriteSheetFrame?[] Copy(RoomBlockPictureTable previous)
    {
        var pictures = new SpriteSheetFrame?[previous.Blocks];
        for (int block = 0; block < pictures.Length; block++)
            pictures[block] = previous.Picture(block);
        return pictures;
    }
}

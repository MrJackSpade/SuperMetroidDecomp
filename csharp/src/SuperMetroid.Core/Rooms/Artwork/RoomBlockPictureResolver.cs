using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves the picture each block a room's planes use shows (#1392, #1431): the block, by its index, over what shows at
/// each run of characters the sheet records it reads, the frame an animation shows there, or else the artwork the
/// background bindings show there. A block is resolved again only when what its characters show changes, so a frame whose
/// bindings stand still reuses its table.
/// </summary>
/// <param name="sheet">The room tileset's block sheet.</param>
public sealed class RoomBlockPictureResolver(RoomBlockSheet sheet)
{
    /// <summary>The BG1 plane whose used blocks <see cref="used"/> holds.</summary>
    private RoomLevelPlane? bg1Plane;

    /// <summary>The layer-2 plane whose used blocks <see cref="used"/> holds, or null.</summary>
    private RoomLevelPlane? bg2Plane;

    /// <summary>The further blocks <see cref="used"/> holds, ascending.</summary>
    private int[] further = [];

    /// <summary>The blocks the planes use, ascending.</summary>
    private int[] used = [];

    /// <summary>What each run of each used block's characters showed when it was last resolved, by used block.</summary>
    private BgArtworkShown[][] shown = [];

    /// <summary>The last table resolved.</summary>
    private RoomBlockPictureTable? table;

    /// <summary>The room tileset's block sheet.</summary>
    public RoomBlockSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>
    /// The pictures the blocks of <paramref name="bg1"/> and <paramref name="bg2"/> show over what
    /// <paramref name="characters"/> show now.
    /// </summary>
    /// <param name="bg1">The room's BG1 plane.</param>
    /// <param name="bg2">The room's layer-2 plane, which shows the same tileset's blocks, or null when BG2 shows none.</param>
    /// <param name="characters">Where the blocks read their characters and what shows over them.</param>
    /// <param name="alsoShown">Further blocks the frame shows, as the X-ray reveal's blocks; none when empty.</param>
    /// <exception cref="InvalidDataException">
    /// A block is not among those the tileset loads, a run of characters a block reads shows bytes no binding names, or the
    /// sheet holds no picture of a block over what its characters show.
    /// </exception>
    public RoomBlockPictureTable Resolve(RoomLevelPlane bg1, RoomLevelPlane? bg2, RoomCharacterSource characters, IEnumerable<int> alsoShown)
    {
        Ensure.NotNull(bg1);
        Ensure.NotNull(characters.Artwork);
        int[] also = [.. Ensure.NotNull(alsoShown).Distinct().Order()];
        RoomBlockPictureTable? previous = table;
        bool fresh = previous is null || !ReferenceEquals(bg1Plane, bg1) || !ReferenceEquals(bg2Plane, bg2) || !also.SequenceEqual(further);
        if (fresh)
        {
            bg1Plane = bg1;
            bg2Plane = bg2;
            further = also;
            used = [.. Uses(bg1).Concat(bg2 is null ? [] : Uses(bg2)).Concat(also).Distinct().Order()];
            shown = [.. used.Select(block => new BgArtworkShown[Sheet.Characters.RunsOf(block).Length])];
        }
        SpriteSheetFrame?[]? pictures = fresh ? new SpriteSheetFrame?[Sheet.Characters.Count] : null;
        for (int index = 0; index < used.Length; index++)
        {
            int block = used[index];
            ImmutableArray<RoomBlockCharacterRun> runs = Sheet.Characters.RunsOf(block);
            BgArtworkShown[] blockShown = shown[index];
            bool changed = fresh;
            for (int run = 0; run < runs.Length; run++)
            {
                BgArtworkShown now = Showing(characters, runs[run]);
                changed |= now != blockShown[run];
                blockShown[run] = now;
            }
            if (!changed)
                continue;
            pictures ??= Copy(previous ?? throw new InvalidOperationException("A changed block was resolved with no table to change."));
            pictures[block] = Sheet.Picture(RoomBlockPictureNames.Of(block, blockShown));
        }
        RoomBlockPictureTable resolved = pictures is null
            ? previous ?? throw new InvalidOperationException("An unchanged resolution has no table.")
            : new RoomBlockPictureTable(Sheet, pictures);
        table = resolved;
        return resolved;
    }

    /// <summary>
    /// What shows over <paramref name="run"/>: the frame an animation that paints its characters selected (#1406), or else
    /// what is bound over them.
    /// </summary>
    private static BgArtworkShown Showing(RoomCharacterSource characters, RoomBlockCharacterRun run) =>
        characters.Animated.TryShowing(run.FirstCharacter, RoomBlockPictureNames.CharacterWords, out BgArtworkShown frame) ? frame
            : characters.Artwork.Showing(run.FirstCharacter, run.CharacterCount);

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

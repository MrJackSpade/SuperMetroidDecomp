using System.Buffers.Binary;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves the picture each block a room's planes use shows (#1392): the block's tile words, from the room's live
/// definitions, over the frame an animation shows at each tile's character, or else the artwork the background bindings show there. A block is resolved again only
/// when its definition or what its characters show changes, so a frame whose bindings stand still reuses its table.
/// </summary>
/// <param name="sheet">The room tileset's block sheet.</param>
public sealed class RoomBlockPictureResolver(RoomBlockSheet sheet)
{
    /// <summary>The definitions the last table was resolved from.</summary>
    private ImmutableArray<byte> definitions;

    /// <summary>The BG1 plane whose used definitions <see cref="used"/> holds.</summary>
    private RoomLevelPlane? bg1Plane;

    /// <summary>The layer-2 plane whose used definitions <see cref="used"/> holds, or null.</summary>
    private RoomLevelPlane? bg2Plane;

    /// <summary>The further definitions <see cref="used"/> holds, ascending.</summary>
    private int[] further = [];

    /// <summary>The definitions the planes use, ascending.</summary>
    private int[] used = [];

    /// <summary>What each used definition's tiles showed when it was last resolved, four per definition.</summary>
    private BgArtworkShown[] shown = [];

    /// <summary>The last table resolved.</summary>
    private RoomBlockPictureTable? table;

    /// <summary>What the definition being resolved shows at each tile's character.</summary>
    private readonly BgArtworkShown[] now = new BgArtworkShown[RoomBlockPictureNames.Tiles];

    /// <summary>The room tileset's block sheet.</summary>
    public RoomBlockSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>
    /// The pictures the blocks of <paramref name="bg1"/> and <paramref name="bg2"/> show over what
    /// <paramref name="characters"/> show now.
    /// </summary>
    /// <param name="bg1">The room's BG1 plane.</param>
    /// <param name="bg2">The room's layer-2 plane, which shares BG1's definitions, or null when BG2 shows none.</param>
    /// <param name="characters">Where the blocks read their characters and what shows over them.</param>
    /// <param name="alsoShown">Further definitions the frame shows, as the X-ray reveal's blocks; none when empty.</param>
    /// <exception cref="InvalidDataException">
    /// A character a block reads shows bytes no binding names, or the sheet holds no picture of a block over what its
    /// characters show.
    /// </exception>
    public RoomBlockPictureTable Resolve(RoomLevelPlane bg1, RoomLevelPlane? bg2, RoomCharacterSource characters, IEnumerable<int> alsoShown)
    {
        Ensure.NotNull(bg1);
        Ensure.NotNull(characters.Artwork);
        int[] also = [.. Ensure.NotNull(alsoShown).Distinct().Order()];
        RoomBlockPictureTable? previous = table;
        bool fresh = previous is null || definitions != bg1.Definitions || !ReferenceEquals(bg1Plane, bg1) || !ReferenceEquals(bg2Plane, bg2)
            || !also.SequenceEqual(further);
        if (fresh)
        {
            definitions = bg1.Definitions;
            bg1Plane = bg1;
            bg2Plane = bg2;
            further = also;
            used = [.. Uses(bg1).Concat(bg2 is null ? [] : Uses(bg2)).Concat(also).Distinct().Order()];
            shown = new BgArtworkShown[used.Length * RoomBlockPictureNames.Tiles];
        }
        SpriteSheetFrame?[]? pictures = fresh ? new SpriteSheetFrame?[definitions.Length / RoomLevelPlane.DefinitionBytes] : null;
        Span<ushort> tiles = stackalloc ushort[RoomBlockPictureNames.Tiles];
        Span<BgArtworkShown> now = this.now;
        for (int index = 0; index < used.Length; index++)
        {
            int definition = used[index];
            bool changed = fresh;
            for (int tile = 0; tile < RoomBlockPictureNames.Tiles; tile++)
            {
                tiles[tile] = BinaryPrimitives.ReadUInt16LittleEndian(
                    definitions.AsSpan(definition * RoomLevelPlane.DefinitionBytes + tile * sizeof(ushort), sizeof(ushort)));
                int character = new SnesBgTilemapWord(tiles[tile]).CharacterIndex;
                // A character an animation paints shows the frame it selected (#1406); any other shows what is bound over it.
                if (!characters.Animated.TryShowing(character, RoomBlockPictureNames.CharacterWords, out now[tile]))
                    now[tile] = characters.Artwork.Showing(character, 1);
                changed |= now[tile] != shown[index * RoomBlockPictureNames.Tiles + tile];
            }
            if (!changed)
                continue;
            now.CopyTo(shown.AsSpan(index * RoomBlockPictureNames.Tiles, RoomBlockPictureNames.Tiles));
            pictures ??= Copy(previous ?? throw new InvalidOperationException("A changed block was resolved with no table to change."));
            pictures[definition] = Sheet.Picture(RoomBlockPictureNames.Of(tiles, now));
        }
        RoomBlockPictureTable resolved = pictures is null
            ? previous ?? throw new InvalidOperationException("An unchanged resolution has no table.")
            : new RoomBlockPictureTable(Sheet, pictures);
        table = resolved;
        return resolved;
    }

    /// <summary>The definitions a plane's blocks use.</summary>
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
        var pictures = new SpriteSheetFrame?[previous.Definitions];
        for (int definition = 0; definition < pictures.Length; definition++)
            pictures[definition] = previous.Picture(definition);
        return pictures;
    }
}

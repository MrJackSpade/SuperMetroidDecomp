using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.LevelData;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One room background plane as its level data shows it: a grid of 16x16 blocks, each a visual level word naming a
/// block definition and its mirroring, over the room's block definitions. Immutable; the room publishes a new plane
/// when gameplay changes a block or a definition, so renderers resolve tiles from level data with no stored tilemap
/// (#1346). Tiles expand through <see cref="LevelBlockTilemapExpander"/>, the one mirroring rule every room view uses.
/// </summary>
public sealed class RoomLevelPlane
{
    /// <summary>Pixels along each side of a level block.</summary>
    public const int BlockPixels = 16;

    /// <summary>Tiles per block definition: top left, top right, bottom left, bottom right.</summary>
    public const int DefinitionTiles = 4;

    /// <summary>The visual level words, row by row.</summary>
    private readonly ushort[] blocks;

    /// <summary>Copies the logical blocks and checks every block names a loaded definition.</summary>
    /// <param name="size">The room's size in blocks.</param>
    /// <param name="blocks">At least <paramref name="size"/>'s blocks of visual level words, row by row; any allocation tail is ignored.</param>
    /// <param name="definitions">The block definitions, <see cref="DefinitionTiles"/> tiles each.</param>
    /// <exception cref="ArgumentException">The blocks do not cover the room, or the definitions are not whole.</exception>
    /// <exception cref="InvalidDataException">A block names a definition the room has not loaded.</exception>
    public RoomLevelPlane(Extent16<RoomBlocks> size, ReadOnlySpan<ushort> blocks, ImmutableArray<BackgroundTile> definitions)
    {
        int count = size.X * size.Y;
        if (count == 0 || blocks.Length < count)
            throw new ArgumentException($"A {size.X}x{size.Y}-block plane needs {count} level words; got {blocks.Length}.", nameof(blocks));
        if (definitions.IsDefaultOrEmpty || definitions.Length % DefinitionTiles != 0)
            throw new ArgumentException("Block definitions are whole groups of four tiles.", nameof(definitions));
        int loaded = definitions.Length / DefinitionTiles;
        foreach (RoomLevelWord word in blocks[..count])
        {
            int definition = word.VisualBlockIndex;
            if (definition >= loaded)
                throw new InvalidDataException($"Level word ${word.Raw:X4} names block ${definition:X3}; the room loaded {loaded}.");
        }
        Size = size;
        this.blocks = blocks[..count].ToArray();
        Definitions = definitions;
    }

    /// <summary>The room's size in blocks.</summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>The visual level words, row by row.</summary>
    public ReadOnlySpan<ushort> Blocks => blocks;

    /// <summary>The room's block definitions, <see cref="DefinitionTiles"/> tiles each, shared by every plane of one publication.</summary>
    public ImmutableArray<BackgroundTile> Definitions { get; }

    /// <summary>The plane's blocks as a frame shows them (#1403), read once.</summary>
    public RoomBlockCells Cells => field ??= new RoomBlockCells(Size, [.. blocks.Select(word =>
    {
        var level = new RoomLevelWord(word);
        return new RoomBlockCell(level.VisualBlockIndex, level.VisualFlipFlags);
    })]);

    /// <summary>The plane's tiles through a scroll register, for simulation that reads the tiles a plane shows.</summary>
    /// <param name="scrollOffset">The scroll register less the layer position.</param>
    public IBgTileSource Tiles(Offset16<BgPlanePixels> scrollOffset) => new ScrolledTiles(this, scrollOffset);

    /// <summary>The plane's tiles through a scroll register.</summary>
    private sealed class ScrolledTiles(RoomLevelPlane plane, Offset16<BgPlanePixels> scrollOffset) : IBgTileSource
    {
        /// <inheritdoc />
        public bool TryTileAt(Position16<BgPlanePixels> scrolled, out BackgroundTile tile) => plane.TryTileAt(scrolled - scrollOffset, out tile);
    }

    /// <summary>The four tiles a block shows, already mirrored by the block's flips.</summary>
    /// <param name="block">The block, inside the room.</param>
    public ExpandedBlockTiles BlockTiles(Position16<RoomBlocks> block) =>
        LevelBlockTilemapExpander.Expand(blocks[block.Y * Size.X + block.X], Definitions.AsSpan());

    /// <summary>The tile shown at a level pixel; false outside the room, where the plane shows nothing.</summary>
    /// <param name="level">The level pixel; coordinates past the room's right or bottom edge, or wrapped below zero, are outside.</param>
    /// <param name="tile">The tile shown there.</param>
    public bool TryTileAt(Position16<BgPlanePixels> level, out BackgroundTile tile)
    {
        if (level.X >= Size.X * BlockPixels || level.Y >= Size.Y * BlockPixels)
        {
            tile = default;
            return false;
        }
        ExpandedBlockTiles tiles = BlockTiles(new Position16<RoomBlocks>(
            (ushort)(level.X / BlockPixels), (ushort)(level.Y / BlockPixels)));
        bool right = level.X % BlockPixels >= ScreenGeometry.BackgroundTileSizePixels;
        bool bottom = level.Y % BlockPixels >= ScreenGeometry.BackgroundTileSizePixels;
        tile = bottom ? right ? tiles.BottomRight : tiles.BottomLeft : right ? tiles.TopRight : tiles.TopLeft;
        return true;
    }
}

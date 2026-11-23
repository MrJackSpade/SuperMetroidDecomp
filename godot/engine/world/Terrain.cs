using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A room's tile grid. It owns every tile's collision, the state of every block, and the drawing of its tiles. The
/// layout is an asset the room names; a room scene never stores its tiles. Terrain does not know the player, enemies
/// or weapons: it is told a rectangle touched it or a hit landed on it, and answers from its tiles' and blocks' rules.
/// </summary>
[Tool]
public partial class Terrain : Node2D, ITerrainShape
{
    /// <summary>The layout's data file, as a path within the assets tree.</summary>
    [Export(PropertyHint.File)]
    public string LayoutFile { get; set; } = "";

    /// <summary>
    /// How far the terrain is shown from where it is, in pixels: a wall that rattles is shown shifted while it does.
    /// It changes nothing about where the terrain is.
    /// </summary>
    public Vector2I ShownShift
    {
        get => new((int)Position.X, (int)Position.Y);
        set => Position = value;
    }

    /// <summary>The loaded layout.</summary>
    private TerrainLayout? _layout;

    /// <summary>The tiles in place right now, by row then column: the layout's, with any patches applied.</summary>
    private TileDefinition[][] _tiles = [];

    /// <summary>The block covering each tile, by row then column; null where there is none.</summary>
    private Block?[][] _blockAt = [];

    /// <summary>Every block, in reading order of its top-left tile.</summary>
    private readonly List<Block> _blocks = [];

    /// <summary>The block types loaded so far, by data file.</summary>
    private readonly Dictionary<string, BlockType> _blockTypes = new(StringComparer.Ordinal);

    /// <summary>The tiles each applied patch replaced, by patch name, for taking it back.</summary>
    private readonly Dictionary<string, TileDefinition[][]> _replacedBy = new(StringComparer.Ordinal);

    /// <summary>Where the layout came from, for loading block types and pictures.</summary>
    private ContentLibrary? _content;

    /// <summary>The loaded layout.</summary>
    /// <exception cref="InvalidOperationException">No layout has been loaded.</exception>
    private TerrainLayout Layout => _layout ?? throw new InvalidOperationException("The terrain has no layout loaded.");

    /// <summary>The width and height of a tile, in pixels.</summary>
    public int TileSize => Layout.TileSize;

    /// <summary>The room's width, in pixels.</summary>
    public int WidthInPixels => Layout.Columns * Layout.TileSize;

    /// <summary>The room's height, in pixels.</summary>
    public int HeightInPixels => Layout.Rows * Layout.TileSize;

    /// <summary>Every block, in reading order.</summary>
    public IReadOnlyList<Block> Blocks => _blocks;

    /// <summary>Loads the layout this terrain names and puts every tile and block in its starting state.</summary>
    public void Load(ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (LayoutFile.Length == 0)
            throw new InvalidOperationException($"The terrain \"{Name}\" names no layout file.");
        _content = content;
        _layout = TerrainLayout.Read(content.ReadData(LayoutFile));
        _tiles = _layout.Tiles.Select(row => row.ToArray()).ToArray();
        _replacedBy.Clear();
        _blocks.Clear();
        RebuildBlocks();
        QueueRedraw();
    }

    /// <summary>Puts a named patch of the layout in place. Applying one already applied changes nothing.</summary>
    /// <exception cref="ArgumentException">The layout has no patch of that name.</exception>
    public void ApplyPatch(string name)
    {
        TerrainPatch patch = PatchNamed(name);
        if (_replacedBy.ContainsKey(name))
            return;
        var replaced = new TileDefinition[patch.Tiles.Length][];
        for (int row = 0; row < patch.Tiles.Length; row++)
        {
            replaced[row] = new TileDefinition[patch.Tiles[row].Length];
            for (int column = 0; column < patch.Tiles[row].Length; column++)
            {
                replaced[row][column] = _tiles[patch.Row + row][patch.Column + column];
                _tiles[patch.Row + row][patch.Column + column] = patch.Tiles[row][column];
            }
        }

        _replacedBy[name] = replaced;
        RebuildBlocks();
        QueueRedraw();
    }

    /// <summary>Takes a named patch back, restoring the tiles it replaced. Taking back one not applied changes nothing.</summary>
    public void RevertPatch(string name)
    {
        TerrainPatch patch = PatchNamed(name);
        if (!_replacedBy.Remove(name, out TileDefinition[][]? replaced))
            return;
        for (int row = 0; row < replaced.Length; row++)
        {
            for (int column = 0; column < replaced[row].Length; column++)
                _tiles[patch.Row + row][patch.Column + column] = replaced[row][column];
        }

        RebuildBlocks();
        QueueRedraw();
    }

    /// <summary>Whether a named patch is in place.</summary>
    public bool IsPatchApplied(string name) => _replacedBy.ContainsKey(name);

    /// <inheritdoc/>
    public bool IsSolidIn(PixelRect rect, SlopeTreatment slopes)
    {
        if (rect.IsEmpty)
            return false;
        if (rect.Left < 0 || rect.Top < 0 || rect.Right > WidthInPixels || rect.Bottom > HeightInPixels)
            return true;

        int size = TileSize;
        for (int row = rect.Top / size; row <= (rect.Bottom - 1) / size; row++)
        {
            for (int column = rect.Left / size; column <= (rect.Right - 1) / size; column++)
            {
                TileDefinition tile = _tiles[row][column];
                switch (tile.Kind)
                {
                    case TileKind.Empty:
                        break;
                    case TileKind.Solid:
                        return true;
                    case TileKind.Block:
                        if (_blockAt[row][column] is { IsSolid: true })
                            return true;
                        break;
                    case TileKind.Slope:
                        if (slopes != SlopeTreatment.Ignore && SlopeIsSolidIn(tile, column, row, rect))
                            return true;
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined tile kind {tile.Kind}.");
                }
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasSlopeIn(PixelRect rect)
    {
        if (rect.IsEmpty)
            return false;
        int size = TileSize;
        int firstRow = Math.Max(0, rect.Top / size), lastRow = Math.Min(Layout.Rows - 1, (rect.Bottom - 1) / size);
        int firstColumn = Math.Max(0, rect.Left / size), lastColumn = Math.Min(Layout.Columns - 1, (rect.Right - 1) / size);
        for (int row = firstRow; row <= lastRow; row++)
        {
            for (int column = firstColumn; column <= lastColumn; column++)
            {
                TileDefinition tile = _tiles[row][column];
                if (tile.Kind == TileKind.Slope && SlopeIsSolidIn(tile, column, row, rect))
                    return true;
            }
        }

        return false;
    }

    /// <summary>The intact blocks <paramref name="rect"/> overlaps or, when <paramref name="touchingCounts"/>, lies against.</summary>
    public IReadOnlyList<Block> BlocksIn(PixelRect rect, bool touchingCounts = false) =>
        _blocks.Where(block => !block.Broken && (touchingCounts ? rect.OverlapsOrTouches(block.Rect) : rect.Overlaps(block.Rect))).ToList();

    /// <summary>
    /// Lands a hit on the terrain within <paramref name="area"/>. Each intact block there is revealed if the hit carries
    /// a kind that reveals it, and broken if it carries a kind that breaks it. Returns the blocks broken.
    /// </summary>
    public IReadOnlyList<Block> TakeHit(Hit hit, PixelRect area)
    {
        var broken = new List<Block>();
        foreach (Block block in BlocksIn(area))
        {
            if (block.Hidden && hit.CarriesAny(block.Type.HiddenUntil))
                block.Reveal();
            if (hit.CarriesAny(block.Type.BrokenBy))
            {
                block.Break();
                broken.Add(block);
            }
        }

        if (broken.Count > 0)
            QueueRedraw();
        return broken;
    }

    /// <summary>Tells the terrain something stands with its feet along <paramref name="feet"/>'s bottom edge.</summary>
    public void StandOn(PixelRect feet)
    {
        foreach (Block block in BlocksIn(new PixelRect(feet.Left, feet.Bottom, feet.Width, 1)))
            block.StandOn();
    }

    /// <summary>
    /// Tells the terrain that a line latched onto <paramref name="block"/> has let go. A block that can be latched
    /// onto only once breaks.
    /// </summary>
    public void LetGoOf(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Type.Contact != BlockContact.LatchOnce || block.Broken)
            return;
        block.Break();
        QueueRedraw();
    }

    /// <summary>Shows every hidden block in <paramref name="area"/> that a revealing ability reveals.</summary>
    public void RevealBySight(PixelRect area)
    {
        foreach (Block block in BlocksIn(area).Where(block => block.Hidden && block.Type.RevealedBySight))
            block.Reveal();
        QueueRedraw();
    }

    /// <summary>
    /// Moves every block's timers on by one update: blocks being stood on give way, and broken ones regrow once their
    /// space is clear.
    /// </summary>
    /// <param name="isOccupied">Says whether something occupies a rectangle of the room, which stops a block regrowing there.</param>
    public void Step(Func<PixelRect, bool> isOccupied)
    {
        bool changed = false;
        foreach (Block block in _blocks)
            changed |= block.Step(spaceIsClear: !isOccupied(block.Rect));
        if (changed)
            QueueRedraw();
    }

    /// <summary>The patch of the given name.</summary>
    private TerrainPatch PatchNamed(string name) =>
        Layout.Patches.TryGetValue(name, out TerrainPatch? patch)
            ? patch
            : throw new ArgumentException($"The layout {LayoutFile} has no patch named \"{name}\".", nameof(name));

    /// <summary>Whether a slope tile has a solid pixel inside a rectangle of the room.</summary>
    private bool SlopeIsSolidIn(TileDefinition tile, int column, int row, PixelRect rect)
    {
        SlopeShape shape = tile.Slope ?? throw new InvalidOperationException("A slope tile has no shape.");
        int size = TileSize;
        int left = column * size, top = row * size;
        return shape.IsSolidIn(
            Math.Max(rect.Left, left) - left, Math.Min(rect.Right, left + size) - left,
            Math.Max(rect.Top, top) - top, Math.Min(rect.Bottom, top + size) - top);
    }

    /// <summary>Makes a fresh block for every block tile in place. A block of several tiles is named by its top-left tile.</summary>
    private void RebuildBlocks()
    {
        ContentLibrary content = _content ?? throw new InvalidOperationException("The terrain has no layout loaded.");
        // A block that is still where it was, and still the same type, keeps its state through a patch.
        var before = _blocks.ToDictionary(block => (block.Rect, block.Type));
        _blocks.Clear();
        _blockAt = _tiles.Select(row => new Block?[row.Length]).ToArray();
        int size = TileSize;
        for (int row = 0; row < _tiles.Length; row++)
        {
            for (int column = 0; column < _tiles[row].Length; column++)
            {
                TileDefinition tile = _tiles[row][column];
                if (tile.Kind != TileKind.Block || _blockAt[row][column] is not null)
                    continue;
                if (!_blockTypes.TryGetValue(tile.Block, out BlockType? type))
                    _blockTypes[tile.Block] = type = BlockType.Read(content.ReadData(tile.Block));
                if (row + tile.BlockHeight > _tiles.Length || column + tile.BlockWidth > _tiles[row].Length)
                    throw new ContentException(LayoutFile, $"the block at column {column}, row {row} runs off the layout.");

                var rect = new PixelRect(column * size, row * size, tile.BlockWidth * size, tile.BlockHeight * size);
                Block block = before.TryGetValue((rect, type), out Block? kept) ? kept : new Block(type, rect);
                _blocks.Add(block);
                for (int y = 0; y < tile.BlockHeight; y++)
                {
                    for (int x = 0; x < tile.BlockWidth; x++)
                    {
                        _blockAt[row + y][column + x] = block;
                        _tiles[row + y][column + x] = tile;
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void _Ready()
    {
        // In the editor a room scene shows its real terrain through the same path the game uses.
        if (Godot.Engine.IsEditorHint() && LayoutFile.Length > 0 && _layout is null)
            Load(ContentLibrary.ForProject());
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_layout is null || _content is null)
            return;
        int size = TileSize;
        for (int row = 0; row < _tiles.Length; row++)
        {
            for (int column = 0; column < _tiles[row].Length; column++)
            {
                TileDefinition tile = _tiles[row][column];
                string picture = tile.Picture;
                if (tile.Kind == TileKind.Block && _blockAt[row][column] is { } block)
                {
                    if (block.Broken || block.Rect.Left != column * size || block.Rect.Top != row * size)
                        continue;
                    picture = block.Hidden ? block.Type.HiddenPicture : block.Type.IntactPicture;
                }

                if (picture.Length > 0)
                    DrawTexture(_content.LoadTexture(picture), new Vector2(column * size, row * size));
            }
        }
    }
}

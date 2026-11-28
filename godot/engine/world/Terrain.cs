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
public partial class Terrain : Node2D, ITerrainShape, Ui.IRecoloured
{
    /// <summary>The colours of its tiles' pictures shown as others, made the first time any is asked for.</summary>
    private Ui.Recolouring? _recolouring;

    /// <inheritdoc/>
    public void Recolour(string source, IReadOnlyList<ColourSwap> swaps) => (_recolouring ??= new Ui.Recolouring(this)).Recolour(source, swaps);

    /// <inheritdoc/>
    public bool ShowsPicturesIn(string place) =>
        _content is { } content && _tiles.Any(row => row.Any(tile => tile.Picture.Length > 0 && Ui.Recolouring.LiesIn(content.Named(tile.Picture), place)));

    /// <summary>The layout's data file, as a path within the assets tree.</summary>
    [Export(PropertyHint.File)]
    public string LayoutFile { get; set; } = "";

    /// <summary>
    /// How far the terrain is shown from where it is, in pixels: a wall that rattles is shown shifted while it does.
    /// It changes nothing about where the terrain is.
    /// </summary>
    public Vector2I ShownShift
    {
        get => new((int)Position.X - _thrown.X, (int)Position.Y - _thrown.Y);
        set => Position = value + _thrown;
    }

    private Vector2I _thrown;

    /// <summary>
    /// How far a shake of the room throws the terrain this update, beyond where it is shown at rest
    /// (<see cref="ShownShift"/>, which it leaves as it is). The room says, each time it shows itself.
    /// </summary>
    internal Vector2I Thrown
    {
        get => _thrown;
        set
        {
            Vector2I rest = ShownShift;
            _thrown = value;
            Position = rest + value;
        }
    }

    /// <summary>The loaded layout.</summary>
    private TerrainLayout? _layout;

    /// <summary>How many updates the terrain has been stepped, and whether any of its tiles steps through pictures.</summary>
    private long _updates;
    private bool _hasMovingTiles;

    /// <summary>The tiles in place right now, by row then column: the layout's, with any patches applied.</summary>
    private TileDefinition[][] _tiles = [];

    /// <summary>The block covering each tile, by row then column; null where there is none.</summary>
    private Block?[][] _blockAt = [];

    /// <summary>The sounds of the blocks that have broken and not yet been asked for: one for each block, in the order they broke.</summary>
    private readonly List<string> _breakSounds = [];
    private readonly List<string> _brokenFacts = [];

    /// <summary>The tiles a revealing ability sees, by row and column, with how many more updates each is shown as seen.</summary>
    private readonly Dictionary<(int Row, int Column), int> _seenTiles = [];

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

    /// <summary>The name of the set of graphics the layout is drawn with; empty when the layout names none.</summary>
    public string Graphics => Layout.Graphics;

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
        _layout = TerrainLayout.Read(content.ReadData(LayoutFile));

        // A block type shared by every set of graphics names its pictures with the set left out: the layout's is put in.
        _content = content.DrawnWith(_layout.Graphics);
        _seenTiles.Clear();
        _updates = 0;
        _hasMovingTiles = _layout.Tiles.Any(row => row.Any(tile => tile.Frames.Count > 0));
        _tiles = _layout.Tiles.Select(row => row.ToArray()).ToArray();
        _replacedBy.Clear();
        _blocks.Clear();
        RebuildBlocks();
        Redraw();
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
        Redraw();
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
        Redraw();
    }

    /// <summary>Whether the layout has a patch of a name.</summary>
    public bool HasPatch(string name) => Layout.Patches.ContainsKey(name);

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
                // A tile a block covers is as solid as the block, whatever it is itself: a block of several tiles
                // covers tiles that are laid as open space.
                if (_blockAt[row][column] is { } block)
                {
                    if (block.IsSolid)
                        return true;
                    continue;
                }

                TileDefinition tile = _tiles[row][column];
                switch (tile.Kind)
                {
                    case TileKind.Empty:
                    case TileKind.Block:
                        break;
                    case TileKind.Solid:
                        return true;
                    case TileKind.Slope:
                        if (SlopeStops(tile, column, row, rect, slopes))
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
    public EdgeStop StopsEdge(PixelRect edge, int across, int down, SlopeTreatment slopes, bool fromRight = false)
    {
        if (edge.IsEmpty)
            return EdgeStop.None;
        int size = TileSize;
        if (down != 0)
        {
            int y = edge.Top, row = TileOf(y, size), top = row * size;
            int middle = edge.Left + edge.Width / 2;
            int firstColumn = TileOf(edge.Left, size), lastColumn = TileOf(edge.Right - 1, size);
            for (int offset = 0; offset <= lastColumn - firstColumn; offset++)
            {
                int column = fromRight ? lastColumn - offset : firstColumn + offset;
                int left = column * size;
                var tile = new PixelRect(left, top, size, size);
                SlopeShape? slope = SlopeOrSolidAt(row, column, out bool solid);
                if (solid)
                    return new EdgeStop(true, down > 0 ? y - top + 1 : top + size - y, false) { Tile = tile };
                if (slope is null || slopes == SlopeTreatment.Ignore || (slope.HoldsMiddle && slopes == SlopeTreatment.IgnoreMiddleHeld))
                    continue;
                if (slope.HoldsMiddle && slopes == SlopeTreatment.ClimbUnderMiddle)
                {
                    // Only the body's middle column meets such a slope; a floor stops only a move down and a ceiling
                    // only a move up, and each stops a move that would end resting exactly against it as well.
                    if (TileOf(middle, size) != column)
                        continue;
                    int solidPixels = slope.SolidAt(middle - left);
                    if (down > 0 && !slope.FromTop && y >= top + size - solidPixels - 1)
                        return new EdgeStop(true, y - (top + size - solidPixels - 1), true) { Tile = tile };
                    // A ceiling's stop says how far back its surface lies, for a mover that rests against it.
                    if (down < 0 && slope.FromTop && y <= top + solidPixels)
                        return new EdgeStop(true, top + solidPixels - y, true) { Tile = tile };
                    continue;
                }

                int first = Math.Max(edge.Left, left) - left, end = Math.Min(edge.Right, left + size) - left;
                if (slope.IsSolidIn(first, end, y - top, y - top + 1))
                {
                    // A shape cut in steps stops the move at the start of the step it would have ended in, and a move
                    // down onto it leaves the body held as sloped ground holds it.
                    int back = slope.Step > 1 ? BackToStep(slope.Step, y - top, down) : BackOutOf(slope, first, end, y - top, y - top + 1, 0, down);
                    return new EdgeStop(true, back, false) { Tile = tile, PutsOnSlope = down > 0 && slope.Step > 1 };
                }
            }

            return EdgeStop.None;
        }

        int x = edge.Left, tileColumn = TileOf(x, size), tileLeft = tileColumn * size;
        for (int row = TileOf(edge.Top, size); row <= TileOf(edge.Bottom - 1, size); row++)
        {
            SlopeShape? slope = SlopeOrSolidAt(row, tileColumn, out bool solid);
            if (solid)
                return new EdgeStop(true, across > 0 ? x - tileLeft + 1 : tileLeft + size - x, false) { Tile = new PixelRect(tileLeft, row * size, size, size) };

            // To a mover that takes it for a whole wall, sloped ground that holds things up by their middle stops the
            // edge as a solid tile does.
            if (slope is { HoldsMiddle: true } && slopes == SlopeTreatment.WallMiddleHeld)
                return new EdgeStop(true, across > 0 ? x - tileLeft + 1 : tileLeft + size - x, false) { Tile = new PixelRect(tileLeft, row * size, size, size) };

            // Sloped ground that holds things up by their middle stops no move across it.
            if (slope is null || slopes == SlopeTreatment.Ignore ||
                (slope.HoldsMiddle && slopes is SlopeTreatment.IgnoreMiddleHeld or SlopeTreatment.ClimbUnderMiddle))
                continue;
            int top = row * size;
            int first = Math.Max(edge.Top, top) - top, end = Math.Min(edge.Bottom, top + size) - top;
            if (slope.IsSolidIn(x - tileLeft, x - tileLeft + 1, first, end))
            {
                int back = slope.Step > 1 ? BackToStep(slope.Step, x - tileLeft, across) : BackOutOf(slope, x - tileLeft, x - tileLeft + 1, first, end, across, 0);
                return new EdgeStop(true, back, false) { Tile = new PixelRect(tileLeft, top, size, size) };
            }
        }

        return EdgeStop.None;
    }

    /// <summary>
    /// What a shot meets that leads with <paramref name="edge"/>: a strip one pixel thick lying across its way, tried
    /// where the shot's move ended. The shot is stopped only when every tile the strip lies in stops it, so one that
    /// grazes open space beside a wall flies on; and its hit lands on every tile reached, whether it is stopped or not.
    /// </summary>
    /// <remarks>
    /// A solid tile and an intact solid block stop it. A slope cut in steps stops it where the strip lies in its solid
    /// part. A slope that holds things up by their middle answers only in the tile in line with the shot's own
    /// middle, and by the middle's place within a tile, not the strip's: where that is in the slope's solid part the
    /// shot is stopped whatever the rest of the strip lies in, and the tiles after it along the strip are not
    /// reached. An edge any part of which lies outside the room meets nothing.
    /// </remarks>
    /// <param name="edge">The strip the shot leads with, one pixel thick.</param>
    /// <param name="across">Whether the shot is moving across, its strip a column of pixels; otherwise down or up, its strip a row.</param>
    /// <param name="middleX">The column of the shot's middle.</param>
    /// <param name="middleY">The row of the shot's middle.</param>
    /// <param name="stopsToo">
    /// Says whether a tile's square that the terrain leaves open stops a shot all the same, as a way out of the room
    /// does: open to whoever leaves by it, it is a wall to a shot.
    /// </param>
    public ShotEdgeMeeting MeetShotEdge(PixelRect edge, bool across, int middleX, int middleY, Func<PixelRect, bool> stopsToo)
    {
        ArgumentNullException.ThrowIfNull(stopsToo);
        if (edge.IsEmpty || edge.Left < 0 || edge.Top < 0 || edge.Right > WidthInPixels || edge.Bottom > HeightInPixels)
            return ShotEdgeMeeting.Nothing;

        int size = TileSize;
        bool everyTileStops = true;

        // Across, the strip lies in one column of tiles and its rows are taken from the top; down, in one row, its
        // columns taken from the left.
        for (int row = edge.Top / size; row <= (edge.Bottom - 1) / size; row++)
        {
            for (int column = edge.Left / size; column <= (edge.Right - 1) / size; column++)
            {
                int left = column * size, top = row * size;
                bool stops;
                if (_blockAt[row][column] is { } block)
                {
                    stops = block.IsSolid;
                }
                else
                {
                    TileDefinition tile = _tiles[row][column];
                    switch (tile.Kind)
                    {
                        case TileKind.Empty:
                        case TileKind.Block:
                            stops = stopsToo(new PixelRect(left, top, size, size));
                            break;
                        case TileKind.Solid:
                            stops = true;
                            break;
                        case TileKind.Slope:
                            SlopeShape slope = tile.Slope ?? throw new InvalidOperationException("A slope tile has no shape.");
                            if (!slope.HoldsMiddle)
                            {
                                stops = slope.IsSolidIn(
                                    Math.Max(edge.Left, left) - left, Math.Min(edge.Right, left + size) - left,
                                    Math.Max(edge.Top, top) - top, Math.Min(edge.Bottom, top + size) - top);
                                break;
                            }

                            bool inLine = across ? TileOf(middleY, size) == row : TileOf(middleX, size) == column;
                            int within = Within(middleX, size), down = Within(middleY, size);
                            if (inLine && slope.IsSolidIn(within, within + 1, down, down + 1))
                            {
                                // The slope's surface ends the strip there: what lies beyond along it is not reached.
                                PixelRect reached = across
                                    ? new PixelRect(edge.Left, edge.Top, edge.Width, Math.Min(edge.Bottom, top + size) - edge.Top)
                                    : new PixelRect(edge.Left, edge.Top, Math.Min(edge.Right, left + size) - edge.Left, edge.Height);
                                return new ShotEdgeMeeting(true, reached);
                            }

                            stops = false;
                            break;
                        default:
                            throw new InvalidOperationException($"Undefined tile kind {tile.Kind}.");
                    }
                }

                everyTileStops &= stops;
            }
        }

        return new ShotEdgeMeeting(everyTileStops, edge);
    }

    /// <summary>Where a pixel column or row lies within its tile, counted from the tile's left or top.</summary>
    private static int Within(int pixel, int size) => ((pixel % size) + size) % size;

    /// <inheritdoc/>
    public int? SlopeRestRow(int x, int y, bool ceiling)
    {
        if (x < 0 || y < 0 || x >= WidthInPixels || y >= HeightInPixels)
            return null;
        int size = TileSize, row = y / size, column = x / size;
        if (SlopeOrSolidAt(row, column, out _) is not { HoldsMiddle: true } slope || slope.FromTop != ceiling)
            return null;
        int solidPixels = slope.SolidAt(x - column * size);
        return ceiling ? row * size + solidPixels : row * size + size - solidPixels - 1;
    }

    /// <summary>The tile row or column a pixel row or column lies in, counted on beyond the room's first for a pixel before it.</summary>
    private static int TileOf(int pixel, int size) => pixel >= 0 ? pixel / size : -((-pixel + size - 1) / size);

    /// <summary>
    /// What stands at a tile for a move tried where it ends: its shape when it is sloped ground, and whether it is
    /// solid all through, as a solid tile, an intact solid block and everything beyond the room are.
    /// </summary>
    private SlopeShape? SlopeOrSolidAt(int row, int column, out bool solid)
    {
        solid = false;
        if (row < 0 || column < 0 || row >= Layout.Rows || column >= Layout.Columns)
        {
            solid = true;
            return null;
        }

        if (_blockAt[row][column] is { } block)
        {
            solid = block.IsSolid;
            return null;
        }

        TileDefinition tile = _tiles[row][column];
        switch (tile.Kind)
        {
            case TileKind.Empty:
            case TileKind.Block:
                return null;
            case TileKind.Solid:
                solid = true;
                return null;
            case TileKind.Slope:
                return tile.Slope ?? throw new InvalidOperationException("A slope tile has no shape.");
            default:
                throw new InvalidOperationException($"Undefined tile kind {tile.Kind}.");
        }
    }

    /// <summary>
    /// How many pixels back, against the way of a move, a strip lying in a sloped tile's solid part comes clear of
    /// it, or out of the tile. The strip is given in the tile's own columns and rows, each range ending just past its
    /// last.
    /// </summary>
    private static int BackOutOf(SlopeShape slope, int firstColumn, int endColumn, int firstRow, int endRow, int across, int down)
    {
        int size = slope.Columns;
        for (int back = 1; ; back++)
        {
            int column = firstColumn - across * back, columnEnd = endColumn - across * back;
            int row = firstRow - down * back, rowEnd = endRow - down * back;
            if (column < 0 || columnEnd > size || row < 0 || rowEnd > size || !slope.IsSolidIn(column, columnEnd, row, rowEnd))
                return back;
        }
    }

    /// <summary>
    /// How many pixels back, against the way of a move, an edge lying in a shape cut in steps comes to the start of
    /// the step it lies in: one past the step's first pixel on the side the move came from. <paramref name="at"/> is
    /// the edge's pixel within the tile along the move.
    /// </summary>
    private static int BackToStep(int step, int at, int way) => way > 0 ? at % step + 1 : step - at % step;

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

    /// <summary>
    /// The shares, in 256ths, that the sloped tiles in the pixel column <paramref name="x"/> between the rows
    /// <paramref name="top"/> and <paramref name="bottom"/> (not included) let through of a move across them on the
    /// ground: one for each such tile that is a floor's and lets less than all of it through, from the top down. A
    /// ceiling's slope takes nothing from a move on the ground.
    /// </summary>
    public IReadOnlyList<int> GroundSharesIn(int x, int top, int bottom)
    {
        var shares = new List<int>();
        if (x < 0 || x >= WidthInPixels || bottom <= top)
            return shares;
        int size = TileSize, column = x / size;
        int firstRow = Math.Max(0, top / size), lastRow = Math.Min(Layout.Rows - 1, (bottom - 1) / size);
        for (int row = firstRow; row <= lastRow; row++)
        {
            if (_blockAt[row][column] is null && _tiles[row][column] is { Kind: TileKind.Slope, Slope: { Share: < SlopeShape.WholeShare, FromTop: false } slope })
                shares.Add(slope.Share);
        }

        return shares;
    }

    /// <summary>
    /// The share, in 256ths, that the sloped ground at a pixel of the room lets through of a move along it, where that
    /// ground holds things up by their middle, a floor's or a ceiling's; all of the move anywhere else. What crawls
    /// along a floor or ceiling goes across by this share of its speed.
    /// </summary>
    public int SlopeShareAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= WidthInPixels || y >= HeightInPixels)
            return SlopeShape.WholeShare;
        int size = TileSize, row = y / size, column = x / size;
        return _blockAt[row][column] is null && _tiles[row][column] is { Kind: TileKind.Slope, Slope: { HoldsMiddle: true } slope }
            ? slope.Share
            : SlopeShape.WholeShare;
    }

    /// <summary>The intact blocks <paramref name="rect"/> overlaps.</summary>
    public IReadOnlyList<Block> BlocksIn(PixelRect rect) =>
        _blocks.Where(block => !block.Broken && rect.Overlaps(block.Rect)).ToList();

    /// <summary>
    /// Lands a hit on the terrain within <paramref name="area"/>. Each intact block there is revealed if the hit carries
    /// a kind that reveals it, and broken if it carries a kind that breaks it. A block broken goes on standing in the
    /// way until the terrain's own step (<see cref="Step"/>), which comes after the player has moved: what struck it
    /// earlier in the update, a shot or a burst, has not opened it to that update's moves. A hit that opens the way
    /// <paramref name="atOnce"/> leaves nothing standing, as what a body breaks by moving into it does. Returns the
    /// blocks broken.
    /// </summary>
    public IReadOnlyList<Block> TakeHit(Hit hit, PixelRect area, bool atOnce = false)
    {
        var broken = new List<Block>();
        bool revealed = false;
        foreach (Block block in BlocksIn(area))
        {
            if (block.Hidden && hit.CarriesAny(block.Type.HiddenUntil))
            {
                block.Reveal();
                revealed = true;
            }

            if (hit.CarriesAny(block.Type.BrokenBy))
            {
                block.Break(standsYet: !atOnce);
                NoteBroken(block, hit);
                broken.Add(block);
            }
        }

        // A block uncovered and left standing looks different from then on, as one broken does.
        if (revealed || broken.Count > 0)
            Redraw();
        return broken;
    }

    /// <summary>Tells the terrain something stands with its feet along <paramref name="feet"/>'s bottom edge.</summary>
    public void StandOn(PixelRect feet)
    {
        foreach (Block block in BlocksIn(new PixelRect(feet.Left, feet.Bottom, feet.Width, 1)))
            block.StandOn();
    }

    /// <summary>Tells the terrain that a line has latched onto <paramref name="block"/>.</summary>
    public void LatchedOnto(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);
        block.LatchOnto();
    }

    /// <summary>
    /// Tells the terrain that a line latched onto <paramref name="block"/> has let go. A block that can be latched
    /// onto only once breaks, unless it is one that breaks so long after it was latched onto.
    /// </summary>
    public void LetGoOf(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Type.Contact != BlockContact.LatchOnce || block.Broken || block.Type.LatchedUpdates > 0)
            return;
        block.Break();
        NoteBroken(block, by: null);
        Redraw();
    }

    /// <summary>Keeps the sound of a block that has just broken, under a hit or under nothing that is one, to be asked for.</summary>
    private void NoteBroken(Block block, Hit? by)
    {
        string sound = block.Type.BreakSoundFor(by);
        if (sound.Length > 0)
            _breakSounds.Add(sound);
        // Its fact is recorded as it breaks only where it shows no breaking; one that does records it after the last
        // picture of that has shown.
        if (block.Type.Records.Length > 0 && block.Type.Breaking is null)
            _brokenFacts.Add(block.Type.Records);
    }

    /// <summary>The facts the blocks that broke since this was last asked record, for the room to record.</summary>
    public IReadOnlyList<string> TakeBrokenFacts()
    {
        string[] facts = [.. _brokenFacts];
        _brokenFacts.Clear();
        return facts;
    }

    /// <summary>Shows every hidden block in <paramref name="area"/> that a revealing ability reveals, through this update.</summary>
    public void RevealBySight(PixelRect area)
    {
        foreach (Block block in BlocksIn(area).Where(block => block.Hidden && block.Type.RevealedBySight))
            block.SeeBySight();
        Redraw();
    }

    /// <summary>
    /// Whether the tile at a point of the room draws a picture as the layout has it: scenery, as against open space.
    /// A point outside the room's tiles has none.
    /// </summary>
    public bool ShowsTileAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= WidthInPixels || y >= HeightInPixels)
            return false;
        TileDefinition tile = _tiles[y / TileSize][x / TileSize];
        return tile.Picture.Length > 0 || tile.Frames.Count > 0;
    }

    /// <summary>
    /// Shows the tile at a point of the room as a revealing ability sees it, through this update, if it looks
    /// otherwise seen: a tile that names a picture for that. Returns whether it does.
    /// </summary>
    public bool SeeTileAt(int x, int y)
    {
        int size = TileSize;
        if (x < 0 || y < 0 || x >= WidthInPixels || y >= HeightInPixels)
            return false;
        (int row, int column) = (y / size, x / size);
        if (_tiles[row][column].SeenPicture is null)
            return false;
        if (!_seenTiles.ContainsKey((row, column)))
            Redraw();
        _seenTiles[(row, column)] = 2;
        return true;
    }

    /// <summary>
    /// Moves every block's timers on by one update: blocks being stood on give way, and broken ones regrow once their
    /// space is clear.
    /// </summary>
    /// <param name="isOccupied">Says whether something occupies a rectangle of the room, which stops a block regrowing there.</param>
    public void Step(Func<PixelRect, bool> isOccupied)
    {
        _updates++;
        bool changed = _hasMovingTiles;

        // A tile seen this update is shown so through it; unseen the next, it looks as it always does again.
        foreach ((int Row, int Column) seen in _seenTiles.Keys.ToList())
        {
            if (--_seenTiles[seen] <= 0)
            {
                _seenTiles.Remove(seen);
                changed = true;
            }
        }

        foreach (Block block in _blocks)
        {
            bool stood = !block.Broken;
            changed |= block.Step(spaceIsClear: !isOccupied(block.Rect));
            if (stood && block.Broken)
                NoteBroken(block, by: null);
            if (block.HasShownItsBreaking && block.Type.Records.Length > 0)
                _brokenFacts.Add(block.Type.Records);
        }

        if (changed)
            Redraw();
    }

    /// <summary>The sounds of the blocks that broke since this was last asked: one for each block, in the order they broke.</summary>
    public IReadOnlyList<string> TakeBreakSounds()
    {
        string[] sounds = [.. _breakSounds];
        _breakSounds.Clear();
        return sounds;
    }

    /// <summary>The patch of the given name.</summary>
    private TerrainPatch PatchNamed(string name) =>
        Layout.Patches.TryGetValue(name, out TerrainPatch? patch)
            ? patch
            : throw new ArgumentException($"The layout {LayoutFile} has no patch named \"{name}\".", nameof(name));

    /// <summary>Whether a slope tile stops a body covering <paramref name="rect"/>, by the way its mover treats slopes.</summary>
    private bool SlopeStops(TileDefinition tile, int column, int row, PixelRect rect, SlopeTreatment slopes)
    {
        bool holdsMiddle = tile.Slope is { HoldsMiddle: true };
        switch (slopes)
        {
            case SlopeTreatment.Ignore:
                return false;
            case SlopeTreatment.IgnoreMiddleHeld when holdsMiddle:
                return false;
            case SlopeTreatment.ClimbUnderMiddle when holdsMiddle:
                // Only the body's middle column meets the slope.
                return SlopeIsSolidIn(tile, column, row, new PixelRect(rect.Left + rect.Width / 2, rect.Top, 1, rect.Height));
            case SlopeTreatment.WholeUnlessMiddleHeld when !holdsMiddle:
                return true;
            case SlopeTreatment.Climb:
            case SlopeTreatment.Wall:
            case SlopeTreatment.ClimbUnderMiddle:
            case SlopeTreatment.IgnoreMiddleHeld:
            case SlopeTreatment.WallMiddleHeld:
            case SlopeTreatment.WholeUnlessMiddleHeld:
                return SlopeIsSolidIn(tile, column, row, rect);
            default:
                throw new InvalidOperationException($"Undefined way {slopes} of treating slopes.");
        }
    }

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
                    // Each tile it covers keeps its own picture; the block only says how solid they are and what they show of it.
                    for (int x = 0; x < tile.BlockWidth; x++)
                        _blockAt[row + y][column + x] = block;
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
    public override void _Draw() => DrawTiles(this);

    /// <summary>
    /// Draws every tile's picture as it is now on <paramref name="on"/>, which may be the terrain itself or something
    /// that shows its tiles again somewhere else (<see cref="LitTiles"/>).
    /// </summary>
    internal void DrawTiles(CanvasItem on)
    {
        if (_layout is null || _content is null)
            return;
        int size = TileSize;
        for (int row = 0; row < _tiles.Length; row++)
        {
            for (int column = 0; column < _tiles[row].Length; column++)
            {
                string picture = PictureShownAt(row, column);
                if (picture.Length > 0)
                    on.DrawTexture(_content.LoadTexture(picture), new Vector2(column * size, row * size));
            }
        }
    }

    /// <summary>
    /// Draws, on <paramref name="front"/>, the part of every tile's picture that lies in front of the things standing
    /// in the room, for the tiles whose picture has such a part beside it.
    /// </summary>
    internal void DrawFront(CanvasItem front)
    {
        if (_layout is null || _content is null)
            return;
        int size = TileSize;
        for (int row = 0; row < _tiles.Length; row++)
        {
            for (int column = 0; column < _tiles[row].Length; column++)
            {
                string picture = PictureShownAt(row, column);
                if (picture.Length > 0 && _content.LoadFrontOf(picture) is { } part)
                    front.DrawTexture(part, new Vector2(column * size, row * size));
            }
        }
    }

    /// <summary>The picture the tile at a row and column shows now; empty for one that shows nothing.</summary>
    private string PictureShownAt(int row, int column)
    {
        TileDefinition tile = _tiles[row][column];
        string picture = tile.SeenPicture is { } seen && _seenTiles.ContainsKey((row, column)) ? seen : tile.PictureAt(_updates);

        // A block is drawn at every tile it covers, as it is now.
        if (_blockAt[row][column] is { } block)
        {
            int size = TileSize;
            int part = (row - block.Rect.Top / size) * (block.Rect.Width / size) + column - block.Rect.Left / size;
            picture = block.PictureNow(picture, part);
        }

        return picture;
    }

    /// <summary>The part of its tiles in front of the room's things, made the first time a layout is loaded.</summary>
    private TerrainFront? _front;

    /// <summary>Has the tiles drawn again, both the part behind the room's things and the part in front of them.</summary>
    private void Redraw()
    {
        QueueRedraw();
        if (_front is null)
        {
            _front = new TerrainFront { Name = "Front", Of = this, ZAsRelative = false, ZIndex = RoomDepth.TilesInFront, UseParentMaterial = true };
            AddChild(_front, forceReadableName: false, InternalMode.Back);
        }

        _front.QueueRedraw();
        ShowBackdropColour();
    }

    /// <summary>The colour shown wherever nothing of the room is drawn, where the layout gives one: one pixel of it, spread over the room.</summary>
    private Sprite2D? _backdropColour;

    /// <summary>
    /// Shows the layout's backdrop colour behind everything, over the whole of the room, or takes it away for a
    /// layout that gives none. It is a picture of that one colour, so it takes the terrain's changes of colour with
    /// the tiles.
    /// </summary>
    private void ShowBackdropColour()
    {
        if (_layout?.Backdrop is not { } colour)
        {
            _backdropColour?.Hide();
            return;
        }

        if (_backdropColour is null)
        {
            _backdropColour = new Sprite2D { Name = "BackdropColour", Centered = false, ZAsRelative = false, ZIndex = RoomDepth.BackdropColour, UseParentMaterial = true };
            AddChild(_backdropColour, forceReadableName: false, InternalMode.Back);
        }

        var pixel = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        pixel.SetPixel(0, 0, colour);
        _backdropColour.Texture = ImageTexture.CreateFromImage(pixel);
        _backdropColour.Scale = new Vector2(_tiles.Length == 0 ? 0 : _tiles[0].Length * TileSize, _tiles.Length * TileSize);
        _backdropColour.Show();
    }
}

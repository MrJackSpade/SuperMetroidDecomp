using Engine.Content;

namespace Engine.World;

/// <summary>
/// A room's tile layout as its file gives it: the tile size, what each letter stands for, the grid of letters, and
/// any named patches. The file is a grid a person can read: one line of text per row of tiles, one letter per tile.
/// </summary>
/// <remarks>
/// <code>
/// {
///   "tile_size": 16,
///   "backdrop": "000000",
///   "tiles": {
///     ".": { "kind": "empty" },
///     "#": { "kind": "solid", "picture": "tiles/rock.png" },
///     "/": { "kind": "slope", "solid": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16], "share": 192, "holds": "middle" },
///     "S": { "kind": "block", "block": "blocks/shot.json" },
///     "~": { "kind": "empty", "pictures": ["tiles/lava_1.png", "tiles/lava_2.png"], "updates": 10 },
///     "w": { "kind": "empty", "picture": "tiles/rock.png", "seen": "" }
///   },
///   "rows": [
///     "........",
///     "...../##",
///     "########"
///   ],
///   "patches": {
///     "bridge gone": { "column": 2, "row": 2, "rows": ["...."] }
///   }
/// }
/// </code>
/// </remarks>
public sealed class TerrainLayout
{
    private TerrainLayout(int tileSize, TileDefinition[][] tiles, IReadOnlyDictionary<string, TerrainPatch> patches, string graphics, Godot.Color? backdrop)
    {
        TileSize = tileSize;
        Tiles = tiles;
        Patches = patches;
        Graphics = graphics;
        Backdrop = backdrop;
    }

    /// <summary>
    /// The colour shown wherever nothing of the room is drawn, which its data gives as "backdrop" in six hexadecimal
    /// digits; null for a layout that gives none, behind which nothing is drawn at all.
    /// </summary>
    public Godot.Color? Backdrop { get; }

    /// <summary>
    /// The name of the set of graphics the layout is drawn with, which its data gives as "graphics"; empty when it
    /// gives none. Things in the room that look different from one set to another are drawn with this one.
    /// </summary>
    public string Graphics { get; }

    /// <summary>The width and height of a tile, in pixels.</summary>
    public int TileSize { get; }

    /// <summary>The tiles, by row then column.</summary>
    public TileDefinition[][] Tiles { get; }

    /// <summary>The named patches.</summary>
    public IReadOnlyDictionary<string, TerrainPatch> Patches { get; }

    /// <summary>How many columns of tiles the layout has.</summary>
    public int Columns => Tiles[0].Length;

    /// <summary>How many rows of tiles the layout has.</summary>
    public int Rows => Tiles.Length;

    /// <summary>Reads a layout from its data file.</summary>
    /// <exception cref="ContentException">The file is missing something or holds something wrong.</exception>
    public static TerrainLayout Read(DataNode data)
    {
        int tileSize = data.Int("tile_size");
        if (tileSize <= 0)
            throw data.Get("tile_size").Problem("must be a positive number of pixels");

        var legend = new Dictionary<char, TileDefinition>();
        foreach ((string letter, DataNode tile) in data.Get("tiles").Entries())
        {
            if (letter.Length != 1)
                throw tile.Problem("is named by more than one letter; a tile is named by exactly one");
            TileDefinition read = ReadTile(tile, tileSize);
            if (tile.Has("pictures"))
            {
                // A tile that moves as it is looked at names its pictures in turn, and how long each shows.
                int updates = tile.Int("updates");
                var frames = tile.List("pictures").Select(picture => picture.AsText()).ToList();
                if (updates <= 0 || frames.Count == 0)
                    throw tile.Problem("names pictures to step through, but none, or no positive number of updates for each");
                read = read with { Frames = frames, FrameUpdates = updates };
            }

            // A tile may look otherwise to a revealing ability: a wall that is air shows as nothing.
            if (tile.Has("seen"))
                read = read with { SeenPicture = tile.Text("seen") };

            legend[letter[0]] = read;
        }

        TileDefinition[][] tiles = ReadRows(data.Get("rows"), legend);
        if (tiles.Length == 0)
            throw data.Get("rows").Problem("has no rows");

        var patches = new Dictionary<string, TerrainPatch>(StringComparer.Ordinal);
        if (data.Has("patches"))
        {
            foreach ((string name, DataNode patch) in data.Get("patches").Entries())
            {
                var read = new TerrainPatch(patch.Int("column"), patch.Int("row"), ReadRows(patch.Get("rows"), legend));
                if (read.Tiles.Length == 0 || read.Column < 0 || read.Row < 0 ||
                    read.Row + read.Tiles.Length > tiles.Length || read.Column + read.Tiles[0].Length > tiles[0].Length)
                    throw patch.Problem("does not lie inside the layout");
                patches[name] = read;
            }
        }

        Godot.Color? backdrop = null;
        if (data.Has("backdrop"))
        {
            string written = data.Text("backdrop");
            if (written.Length != 6 || !written.All(Uri.IsHexDigit))
                throw data.Get("backdrop").Problem("is not a colour written as six hexadecimal digits");
            backdrop = new Godot.Color(
                Convert.ToInt32(written[..2], 16) / 255f, Convert.ToInt32(written[2..4], 16) / 255f, Convert.ToInt32(written[4..], 16) / 255f);
        }

        return new TerrainLayout(tileSize, tiles, patches, data.Has("graphics") ? data.Text("graphics") : "", backdrop);
    }

    /// <summary>Reads what one letter stands for.</summary>
    private static TileDefinition ReadTile(DataNode tile, int tileSize)
    {
        TileKind kind = tile.Get("kind").AsEnum<TileKind>();
        string picture = tile.Has("picture") ? tile.Text("picture") : "";
        switch (kind)
        {
            case TileKind.Empty:
            case TileKind.Solid:
                return new TileDefinition(kind, null, "", 1, 1, picture);
            case TileKind.Slope:
                int[] solid = tile.List("solid").Select(column => column.AsInt()).ToArray();
                if (solid.Length != tileSize)
                    throw tile.Get("solid").Problem($"must list {tileSize} columns, one per pixel of a tile's width");
                if (solid.Any(pixels => pixels < 0 || pixels > tileSize))
                    throw tile.Get("solid").Problem($"must hold 0 through {tileSize} in each column");
                bool fromTop = tile.Has("from") && tile.Get("from").AsEnum<SlopeAnchor>() == SlopeAnchor.Top;
                int share = tile.Has("share") ? tile.Int("share") : SlopeShape.WholeShare;
                if (share is < 1 or > SlopeShape.WholeShare)
                    throw tile.Get("share").Problem($"must be 1 through {SlopeShape.WholeShare}, the 256ths of a move along the slope that get through");
                bool holdsMiddle = tile.Has("holds");
                if (holdsMiddle && tile.Text("holds") != "middle")
                    throw tile.Get("holds").Problem("must be \"middle\", for a slope that holds things up by their middle, or be left out");
                int step = tile.Has("step") ? tile.Int("step") : 1;
                if (step < 1 || step > tileSize)
                    throw tile.Get("step").Problem($"must be 1 through {tileSize}, the pixels of each step the shape is cut in");
                return new TileDefinition(kind, new SlopeShape(solid, fromTop, share, holdsMiddle, step), "", 1, 1, picture);
            case TileKind.Block:
                int width = tile.Has("width") ? tile.Int("width") : 1;
                int height = tile.Has("height") ? tile.Int("height") : 1;
                if (width < 1 || height < 1)
                    throw tile.Problem("is a block of no tiles: its width and height are each at least 1");
                return new TileDefinition(kind, null, tile.Text("block"), width, height, picture);
            default:
                throw tile.Get("kind").Problem("is not a kind of tile");
        }
    }

    /// <summary>Reads a grid of letters into tiles.</summary>
    private static TileDefinition[][] ReadRows(DataNode rows, Dictionary<char, TileDefinition> legend)
    {
        var read = new List<TileDefinition[]>();
        foreach (DataNode row in rows.Items())
        {
            string letters = row.AsText();
            if (read.Count > 0 && letters.Length != read[0].Length)
                throw row.Problem($"is {letters.Length} tiles wide, but the first row is {read[0].Length}");
            read.Add(letters.Select(letter => legend.TryGetValue(letter, out TileDefinition? tile)
                ? tile
                : throw row.Problem($"uses the letter \"{letter}\", which \"tiles\" does not define")).ToArray());
        }

        return [.. read];
    }
}

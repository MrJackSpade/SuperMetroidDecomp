using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>An immutable Mode 7 map a frame shows (#1349).</summary>
public sealed class Mode7MapSnapshot
{
    /// <summary>The tile numbers, row-major.</summary>
    private readonly byte[] cells;

    /// <summary>Copies the map's tiles.</summary>
    /// <param name="cells">Exactly <see cref="Mode7Map.Cells"/> tile numbers, row-major.</param>
    /// <param name="tileset">The room tileset whose Mode 7 characters the tile numbers select.</param>
    /// <param name="tiles">The pre-coloured tiles the map draws from, or null where it draws from VRAM's characters (#1392).</param>
    public Mode7MapSnapshot(ReadOnlySpan<byte> cells, TilesetDefinition tileset, Mode7TileSheet? tiles = null)
    {
        Ensure.LengthEqual(cells, Mode7Map.Cells);
        this.cells = cells.ToArray();
        Tileset = tileset;
        Tiles = tiles;
    }

    /// <summary>The pre-coloured tiles the map draws from, or null where it draws from VRAM's characters (#1392).</summary>
    public Mode7TileSheet? Tiles { get; }

    /// <summary>The room tileset whose Mode 7 characters the tile numbers select: the map's artwork binding.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The tile numbers, row-major.</summary>
    public ReadOnlySpan<byte> Cells => cells;

    /// <summary>The tiles the map names, each once.</summary>
    public IReadOnlySet<byte> NamedTiles => named ??= [.. cells];

    /// <summary>The tiles the map names, once found.</summary>
    private HashSet<byte>? named;

    /// <summary>The tile number at a map cell, column and row 0 through 127.</summary>
    public byte TileAt(Position16<TilemapCells> cell) => cells[cell.Y * Mode7Map.Side + cell.X];
}

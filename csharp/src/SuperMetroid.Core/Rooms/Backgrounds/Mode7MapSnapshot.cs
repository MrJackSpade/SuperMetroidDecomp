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
    public Mode7MapSnapshot(ReadOnlySpan<byte> cells, TilesetDefinition tileset)
    {
        Ensure.LengthEqual(cells, Mode7Map.Cells);
        this.cells = cells.ToArray();
        Tileset = tileset;
    }

    /// <summary>The room tileset whose Mode 7 characters the tile numbers select: the map's artwork binding.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The tile numbers, row-major.</summary>
    public ReadOnlySpan<byte> Cells => cells;

    /// <summary>The tile number at a map cell, column and row 0 through 127.</summary>
    public byte TileAt(Position16<TilemapCells> cell) => cells[cell.Y * Mode7Map.Side + cell.X];
}

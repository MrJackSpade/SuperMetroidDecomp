namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>
/// The menu atlas tiles that hold the cells of one calculated menu picture (#1481): the import pipeline's knowledge of
/// where each cell sits in the atlas. The cell identities' own values are arbitrary.
/// </summary>
/// <typeparam name="TCell">The picture's cells.</typeparam>
internal sealed class MenuAtlasCells<TCell>
    where TCell : struct, Enum
{
    /// <summary>Each atlas tile's cell.</summary>
    private readonly Dictionary<int, TCell> _cells = [];

    /// <summary>Each cell's atlas tile.</summary>
    private readonly Dictionary<TCell, int> _tiles;

    /// <summary>Keeps <paramref name="tiles"/>, requiring one distinct tile for every cell.</summary>
    internal MenuAtlasCells(IReadOnlyDictionary<TCell, int> tiles)
    {
        _tiles = new Dictionary<TCell, int>(tiles);
        foreach (TCell cell in Enum.GetValues<TCell>())
            if (!_tiles.ContainsKey(cell))
                throw new InvalidDataException($"{typeof(TCell).Name}.{cell} has no menu atlas tile.");
        foreach (KeyValuePair<TCell, int> entry in _tiles)
            if (!_cells.TryAdd(entry.Value, entry.Key))
                throw new InvalidDataException($"Menu atlas tile {entry.Value} holds both {_cells[entry.Value]} and {entry.Key}.");
    }

    /// <summary>The atlas tiles of every cell.</summary>
    internal IEnumerable<int> Tiles => _tiles.Values;

    /// <summary>Whether <paramref name="tile"/> holds one of the cells.</summary>
    internal bool Contains(int tile) => _cells.ContainsKey(tile);

    /// <summary>The cell <paramref name="tile"/> holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tile"/> holds none of the cells.</exception>
    internal TCell Cell(int tile) => _cells.TryGetValue(tile, out TCell cell)
        ? cell
        : throw new ArgumentOutOfRangeException(nameof(tile), tile, $"Menu atlas tile holds no {typeof(TCell).Name}.");

    /// <summary>The atlas tile holding <paramref name="cell"/>.</summary>
    internal int Tile(TCell cell) => _tiles[cell];
}

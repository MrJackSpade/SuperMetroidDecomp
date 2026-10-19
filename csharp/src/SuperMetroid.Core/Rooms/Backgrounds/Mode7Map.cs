using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The 128 by 128 Mode 7 tile map a Ceres room shows (#1349): seeded from the room tileset's map, then patched by the
/// Ceres door's platform frames and Ridley's getaway transfer lists. Room state; each NMI publishes a
/// <see cref="Mode7MapSnapshot"/>, so nothing writes the map into VRAM.
/// </summary>
public sealed class Mode7Map
{
    /// <summary>Tiles along each side of the map.</summary>
    public const int Side = 128;

    /// <summary>Tiles in the map.</summary>
    public const int Cells = Side * Side;

    /// <summary>The map's tile numbers, row-major.</summary>
    private readonly byte[] cells;

    /// <summary>The snapshot of the current tiles, until the next write.</summary>
    [NonSerialized] private Mode7MapSnapshot? published;

    /// <summary>Takes the room tileset's map.</summary>
    /// <param name="cells">Exactly <see cref="Cells"/> tile numbers, row-major.</param>
    /// <param name="tileset">The room tileset whose Mode 7 characters the map's tile numbers select: its artwork binding.</param>
    public Mode7Map(ReadOnlySpan<byte> cells, TilesetDefinition tileset)
    {
        Ensure.LengthEqual(cells, Cells);
        this.cells = cells.ToArray();
        Tileset = tileset;
    }

    /// <summary>The room tileset whose Mode 7 characters the map's tile numbers select.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>Writes <paramref name="tiles"/> from map cell <paramref name="firstCell"/> (row-major) onward.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tiles run past the map.</exception>
    public void Write(int firstCell, ReadOnlySpan<byte> tiles)
    {
        if (firstCell < 0 || firstCell + tiles.Length > Cells)
            throw new ArgumentOutOfRangeException(nameof(firstCell), firstCell, $"{tiles.Length} tiles do not fit the Mode 7 map there.");
        tiles.CopyTo(cells.AsSpan(firstCell));
        published = null;
    }

    /// <summary>The map as the next frame shows it; shared until the next write.</summary>
    public Mode7MapSnapshot Publish() => published ??= new Mode7MapSnapshot(cells, Tileset);
}

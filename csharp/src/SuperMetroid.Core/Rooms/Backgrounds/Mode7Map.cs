using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The 128 by 128 Mode 7 tile map a Ceres room shows (#1349): seeded from the room tileset's map, then patched by the
/// Ceres door's platform frames and Ridley's getaway transfer lists. Room state; each NMI publishes it as a
/// <see cref="AffineFloor"/>, so nothing writes the map into VRAM.
/// </summary>
public sealed class Mode7Map
{
    /// <summary>Tiles along each side of the map.</summary>
    public const int Side = 128;

    /// <summary>Tiles in the map.</summary>
    public const int Cells = Side * Side;

    /// <summary>The map's tile numbers, row-major.</summary>
    private readonly byte[] cells;

    /// <summary>The floor of the current tiles, until the next write.</summary>
    [NonSerialized] private AffineFloor? published;

    /// <summary>The tiles the map names, until the next write.</summary>
    [NonSerialized] private HashSet<byte>? named;

    /// <summary>Takes the room tileset's map.</summary>
    /// <param name="cells">Exactly <see cref="Cells"/> tile numbers, row-major.</param>
    /// <param name="tileset">The room tileset whose Mode 7 characters the map's tile numbers select: its artwork binding.</param>
    /// <param name="tiles">The tileset's pre-coloured tiles the map draws from (#1392).</param>
    public Mode7Map(ReadOnlySpan<byte> cells, TilesetDefinition tileset, Mode7TileSheet tiles)
    {
        Ensure.LengthEqual(cells, Cells);
        this.cells = cells.ToArray();
        Tileset = tileset;
        Tiles = Ensure.NotNull(tiles);
    }

    /// <summary>The tileset's pre-coloured tiles the map draws from (#1392).</summary>
    public Mode7TileSheet Tiles { get; }

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
        named = null;
    }

    /// <summary>The tiles the map names, each once.</summary>
    public IReadOnlySet<byte> NamedTiles => named ??= [.. cells];

    /// <summary>The floor the next frame shows, drawn from the tileset's pre-coloured tiles; shared until the next write.</summary>
    public AffineFloor Publish() => published ??= AffineFloor.Of(cells, Mode7TilePictures.Of(Tiles));
}

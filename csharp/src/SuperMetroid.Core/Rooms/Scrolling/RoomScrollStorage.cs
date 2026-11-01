using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The 50-byte room scroll-zone allocation. It is session-scoped
/// and survives room changes: an implicit scroll table writes only the new room's cells, and the scroll routines read
/// the previous room's bytes past them. Each room's <see cref="RoomScrollGrid"/> binds to it. A camera check that reads
/// past the cells reads the session's stored explored maps.
/// </summary>
/// <param name="storedExploredMaps">The session's stored explored maps, which a camera check past the cells reads.</param>
public sealed class RoomScrollStorage(StoredExploredMaps<AreaId> storedExploredMaps)
{
    /// <summary>The allocation's bytes, one scroll state per 256-pixel screen, row-major.</summary>
    private readonly byte[] _bytes = new byte[RoomScrollGrid.StorageByteCount];

    /// <summary>The session's stored explored maps, which a camera check past the cells reads.</summary>
    private readonly StoredExploredMaps<AreaId> _storedExploredMaps = Ensure.NotNull(storedExploredMaps);

    /// <summary>One cell's raw scroll byte, by allocation index.</summary>
    internal byte this[int index]
    {
        get => _bytes[index];
        set => _bytes[index] = value;
    }

    /// <summary>Copies a room's compiled scroll table over the whole allocation.</summary>
    internal void Load(ReadOnlySpan<byte> compiled) => compiled.CopyTo(_bytes);

    /// <summary>
    /// The scroll state a camera check reads <paramref name="cellsPast"/> cells past the allocation: the run of the
    /// stored explored maps it lands on, read as a red scroll when none of the run is explored, a blue scroll when only
    /// its last cell is, and a green scroll otherwise.
    /// </summary>
    /// <param name="cellsPast">How far past the last cell the check indexes, zero for the first.</param>
    /// <exception cref="ArgumentOutOfRangeException">The check indexes past every stored explored map.</exception>
    internal RoomScrollState ReadPastCells(int cellsPast) =>
        _storedExploredMaps.ReadPastScrollCells(cellsPast) switch
        {
            StoredExploredMaps<AreaId>.RunReading.NothingExplored => RoomScrollState.RedBoundary,
            StoredExploredMaps<AreaId>.RunReading.OnlyLastCellExplored => RoomScrollState.Blue,
            StoredExploredMaps<AreaId>.RunReading.OtherwiseExplored => RoomScrollState.Green,
            var reading => throw new ArgumentOutOfRangeException(nameof(cellsPast), reading, "Unknown stored-map reading."),
        };
}

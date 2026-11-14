using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Verification access to <see cref="RoomScrollGrid"/>: whole-grid views and grids built from native tables.</summary>
internal static class RoomScrollGridAccess
{
    /// <summary>Exposes every kept cell, including those past a short room's dimensions.</summary>
    extension(RoomScrollGrid self)
    {
        /// <summary>
        /// Every kept cell, including those past the room's own. The original explicit loader at <c>$82:E878-$82:E889</c>
        /// copies the whole table unconditionally, so edge reads can observe data following a shorter table rather than
        /// an invented red padding.
        /// </summary>
        internal RoomScrollState[] KeptStates => [.. Enumerable.Range(0, self.KeptCells.Count).Select(self.KeptCells.Read)];
    }

    /// <summary>Builds grids for checks.</summary>
    extension(RoomScrollGrid)
    {
        /// <summary>The session's scroll cells as power-on leaves them, with every stored explored map unexplored.</summary>
        internal static ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> PowerOnCells() =>
            RoomScrollGrid.KeepCellsAcrossRooms(StoredExploredMapLoads.PowerOn());

        /// <summary>
        /// Loads a native explicit table of <see cref="RoomScrollGrid.KeptCellCount"/> scroll bytes into fresh power-on
        /// cells, each byte read as the camera reads it.
        /// </summary>
        internal static RoomScrollGrid LoadNativeTable(ReadOnlySpan<byte> table, Extent16<RoomScreens> size) =>
            RoomScrollGrid.LoadNativeTable(RoomScrollGrid.PowerOnCells(), table, size);

        /// <summary>Loads a native explicit table of scroll bytes into <paramref name="cells"/>, each byte read as the camera reads it.</summary>
        internal static RoomScrollGrid LoadNativeTable(
            ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> cells, ReadOnlySpan<byte> table, Extent16<RoomScreens> size) =>
            RoomScrollGrid.FromDefinition(cells, new RoomScrollDefinition("native table", RoomScrollStates.FromStorageBytes(table)), size);
    }
}

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where a room-object draw's run places its cells (#1557).</summary>
internal static class RoomObjectRuns
{
    /// <summary>Where <paramref name="run"/>'s cell <paramref name="index"/> lies from the draw's origin.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not one of the run's cells.</exception>
    internal static Offset16<RoomBlocks> CellAt(this RoomObjectShotBlockDrawDefinitions.Run run, int index)
    {
        if ((uint)index >= (uint)run.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return run.Start + (run.Direction == RoomObjectRunDirection.Vertical
            ? new Offset16<RoomBlocks>(0, index)
            : new Offset16<RoomBlocks>(index, 0));
    }
}

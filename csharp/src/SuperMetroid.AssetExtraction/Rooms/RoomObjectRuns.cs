using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Where a run of the cartridge's room-object draw list places its cells (#1557, #1862).</summary>
internal static class RoomObjectRuns
{
    /// <summary>Where <paramref name="run"/>'s cell <paramref name="index"/> lies from the draw's origin.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not one of the run's cells.</exception>
    internal static Offset16<RoomBlocks> CellAt(this RoomObjectDrawLists.Run run, int index)
    {
        if ((uint)index >= (uint)run.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return run.Start + (run.Direction == RoomObjectRunDirection.Vertical
            ? new Offset16<RoomBlocks>(0, index)
            : new Offset16<RoomBlocks>(index, 0));
    }
}

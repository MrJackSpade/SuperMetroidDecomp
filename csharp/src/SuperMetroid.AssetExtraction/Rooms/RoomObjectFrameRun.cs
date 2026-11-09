using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One run of a room-object frame as the import checks and pictures it (#1557): the direction it writes in, how each of
/// its cells collides, and where its first cell lies from the object's origin. What the cells show is read from the
/// cartridge, never from the game.
/// </summary>
/// <param name="Direction">The direction the run writes its cells in.</param>
/// <param name="Collisions">How each of the run's cells collides, in write order.</param>
/// <param name="Start">Where the run's first cell lies, in blocks from the object's origin.</param>
internal readonly record struct RoomObjectFrameRun(
    RoomObjectRunDirection Direction,
    ReadOnlyMemory<RoomCollisionType> Collisions,
    Offset16<RoomBlocks> Start)
{
    /// <summary>The number of cells the run writes.</summary>
    internal int Count => Collisions.Length;

    /// <summary>Where the run's cell <paramref name="index"/> lies from the object's origin.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not one of the run's cells.</exception>
    internal Offset16<RoomBlocks> CellAt(int index)
    {
        if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
        return Start + (Direction == RoomObjectRunDirection.Vertical
            ? new Offset16<RoomBlocks>(0, index)
            : new Offset16<RoomBlocks>(index, 0));
    }
}

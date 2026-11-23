using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.WreckedShip;

/// <summary>
/// The row of blocks a Wrecked Ship treadmill writes, from its first block (<see cref="WreckedShipTreadmillBlockDefinitions"/>):
/// checked once, as the treadmill spawns, to fit its room's block row, so the belt it runs writes no block that wraps.
/// </summary>
internal readonly partial record struct WreckedShipTreadmillRow
{
    /// <summary>Starts the row at <paramref name="origin"/>.</summary>
    private WreckedShipTreadmillRow(Position16<RoomBlocks> origin) => Origin = origin;

    /// <summary>The row's first block.</summary>
    internal Position16<RoomBlocks> Origin { get; }

    /// <summary>The treadmill's row in <paramref name="level"/>.</summary>
    /// <exception cref="InvalidDataException">The row would wrap into the next room block row.</exception>
    internal static WreckedShipTreadmillRow In(RoomLevelData level)
    {
        Position16<RoomBlocks> origin = level.GridBlock(
            new(WreckedShipTreadmillBlockDefinitions.BlockX, WreckedShipTreadmillBlockDefinitions.BlockY));
        int count = WreckedShipTreadmillBlockDefinitions.BlockCount;
        if (origin.X + count > level.Size.X)
        {
            throw new InvalidDataException(
                $"Wrecked Ship treadmill row ({origin.X},{origin.Y}).." +
                $"({origin.X + count - 1},{origin.Y}) exceeds a {level.Size.X}-block room row.");
        }
        return new(origin);
    }
}

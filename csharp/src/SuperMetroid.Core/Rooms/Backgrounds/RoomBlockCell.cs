using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>One 16x16 block of a room plane as a frame shows it (#1403, #1431): the block it shows and its mirroring.</summary>
/// <param name="Definition">The block shown, by its index among those the room loads.</param>
/// <param name="Flips">How the block is mirrored.</param>
public readonly record struct RoomBlockCell(int Definition, LevelBlockFlipFlags Flips)
{
    /// <summary>The block shown, by its index among those the room loads; zero or more.</summary>
    public int Definition
    {
        get;
        init => field = Ensure.BetweenInclusive(0, int.MaxValue, value);
    } = Ensure.BetweenInclusive(0, int.MaxValue, Definition);
}

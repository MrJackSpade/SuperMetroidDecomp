using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres pre-elevator hall's falling-debris room main: the updates until the next falling tile. The first wait is not
/// set up when the room loads; it is what the previous room's main left, normally the elevator shaft's last turn step
/// (<see cref="ShaftTurnSetsDebrisWait"/>), and every later tile falls <see cref="FallInterval"/> updates after the one before it.
/// </summary>
[RoomLifetime]
public sealed class CeresFallingDebrisRoomMainState
{
    /// <summary>The updates from one falling tile to the next once the first has fallen: a tile falls on every ninth update.</summary>
    private const int FallInterval = 9;

    /// <summary>The hand-over the hall reads its first wait from and leaves its countdown in for the following room's main.</summary>
    private readonly ShaftTurnSetsDebrisWait _handOver;

    /// <summary>
    /// The wait for the next falling tile, over on the update it falls: at first the wait the previous room's main left, plus
    /// the update it falls on.
    /// </summary>
    private readonly UpdateCountdown nextFall;

    /// <summary>Builds one room's debris owner, adopting the wait the previous room's main left.</summary>
    /// <param name="handOver">The shaft's turn, which sets the hall's first wait, and the hall's countdown left for the next room.</param>
    public CeresFallingDebrisRoomMainState(ShaftTurnSetsDebrisWait handOver)
    {
        _handOver = Ensure.NotNull(handOver);
        // The hall reads the value it inherits as a countdown that drops a tile once a step takes it past zero: the tile
        // falls on the update after the count runs out, or on the first update when the count already reads as run past
        // zero after that update's step (the shaft's backward sweep leaves its steps reading so).
        WrappingCountdown inherited = handOver.FirstDebrisWait;
        nextFall = UpdateCountdown.Lasting(inherited.Decrement().IsNegative ? 1 : inherited.RemainingFrames + 1);
    }

    /// <summary>Counts down one update and reports whether a tile falls on it, starting the next interval when one does.</summary>
    internal bool Step()
    {
        bool falls = nextFall.Count();
        if (falls)
            nextFall.Restart(FallInterval);
        // The following room's main reads what the hall leaves as the hall reads it: the updates before the one its next
        // tile falls on.
        _handOver.LeaveDebrisWait(WrappingCountdown.Start(nextFall.UpdatesLeft - 1));
        return falls;
    }
}

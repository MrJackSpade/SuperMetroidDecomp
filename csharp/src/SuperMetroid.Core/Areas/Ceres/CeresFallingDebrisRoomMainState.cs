using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres pre-elevator hall's falling-debris room main: the updates until the next falling tile. The first wait is not
/// set up when the room loads; it is what the previous room's main left (<see cref="RoomMainCarryover"/>), normally the
/// elevator shaft's last turn step, and every later tile falls <see cref="FallInterval"/> updates after the one before it.
/// </summary>
[RoomLifetime]
public sealed class CeresFallingDebrisRoomMainState
{
    /// <summary>The updates from one falling tile to the next once the first has fallen: a tile falls on every ninth update.</summary>
    private const int FallInterval = 9;

    /// <summary>The value the following room's main inherits.</summary>
    private readonly RoomMainCarryover _carryover;

    /// <summary>The updates until the next tile falls, the one it falls on included: 1 through 32769.</summary>
    private int _updatesToNextFall;

    /// <summary>Builds one room's debris owner, adopting the wait the previous room's main left.</summary>
    /// <param name="carryover">The room-main value carried between rooms.</param>
    public CeresFallingDebrisRoomMainState(RoomMainCarryover carryover)
    {
        _carryover = Ensure.NotNull(carryover);
        // The hall reads the value it inherits as a countdown that drops a tile once a step takes it past zero: the tile
        // falls on the update after the count runs out, or on the first update when the count already reads as run past
        // zero after that update's step (the shaft's backward sweep leaves its steps reading so).
        WrappingCountdown inherited = carryover.Countdown;
        _updatesToNextFall = inherited.Decrement().IsNegative ? 1 : inherited.RemainingFrames + 1;
    }

    /// <summary>Counts down one update and reports whether a tile falls on it, starting the next interval when one does.</summary>
    internal bool Step()
    {
        bool falls = --_updatesToNextFall == 0;
        if (falls)
            _updatesToNextFall = FallInterval;
        // The following room's main reads what the hall leaves as the hall reads it: the updates before the one its next
        // tile falls on.
        _carryover.LeaveCountdown(WrappingCountdown.Start(_updatesToNextFall - 1));
        return falls;
    }
}

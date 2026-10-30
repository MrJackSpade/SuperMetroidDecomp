using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres pre-elevator hall's falling-debris room main (<c>$8F:E525</c>): its countdown to the next falling tile.
/// The countdown is not set up when the room loads; it starts from what the previous room's main left
/// (<see cref="RoomMainCarryover"/>), normally the elevator shaft's last turn step.
/// </summary>
[RoomLifetime]
public sealed class CeresFallingDebrisRoomMainState
{
    /// <summary>Updates between falling tiles once the first has fallen.</summary>
    private const int SpawnInterval = 8;

    /// <summary>The value the following room's main inherits.</summary>
    private readonly RoomMainCarryover _carryover;

    /// <summary>Builds one room's debris owner, adopting the countdown the previous room's main left.</summary>
    /// <param name="carryover">The room-main value carried between rooms.</param>
    public CeresFallingDebrisRoomMainState(RoomMainCarryover carryover)
    {
        _carryover = Ensure.NotNull(carryover);
        SpawnTimer = carryover.Countdown;
    }

    /// <summary>The countdown to the next falling tile; a tile falls on the update it passes zero.</summary>
    public WrappingCountdown SpawnTimer { get; private set; }

    /// <summary>Counts down one update and reports whether a tile falls on it, restarting the countdown when one does.</summary>
    internal bool Step()
    {
        CountdownStep timer = SpawnTimer.Decrement();
        SpawnTimer = timer.IsNonNegative ? timer.Remaining : WrappingCountdown.Start(SpawnInterval);
        _carryover.LeaveCountdown(SpawnTimer);
        return !timer.IsNonNegative;
    }
}

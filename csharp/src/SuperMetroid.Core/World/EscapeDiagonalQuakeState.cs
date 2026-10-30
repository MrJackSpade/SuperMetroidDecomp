using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// An escape room's diagonal-quake countdown, kept by the two shaking room mains: while it runs the room shakes
/// diagonally, and the horizontal shake returns when it ends. Only two room setups clear it; any other escape room
/// starts from what the previous room's main left (<see cref="RoomMainCarryover"/>).
/// </summary>
[RoomLifetime]
internal sealed class EscapeDiagonalQuakeState
{
    /// <summary>The value the following room's main inherits.</summary>
    private readonly RoomMainCarryover _carryover;

    /// <summary>Builds one room's countdown, adopting the value the previous room's main left.</summary>
    /// <param name="carryover">The room-main value carried between rooms.</param>
    internal EscapeDiagonalQuakeState(RoomMainCarryover carryover)
    {
        _carryover = Ensure.NotNull(carryover);
        Countdown = carryover.Countdown;
    }

    /// <summary>The updates left of the diagonal quake; zero while the room shakes horizontally.</summary>
    internal WrappingCountdown Countdown
    {
        get;
        set
        {
            field = value;
            _carryover.LeaveCountdown(value);
        }
    }
}

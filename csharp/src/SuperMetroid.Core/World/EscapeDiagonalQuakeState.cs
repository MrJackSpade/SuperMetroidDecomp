using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// An escape room's diagonal-quake countdown, kept by the two shaking room mains: while it runs the room shakes
/// diagonally, and the horizontal shake returns when it ends. The countdown is the working value room mains carry between
/// rooms (<see cref="RoomMainCarryover"/>): only two room setups clear it, so any other escape room carries on what the
/// previous room's main left.
/// </summary>
/// <param name="carryover">The room-main value carried between rooms, which the countdown is read from and left in.</param>
[RoomLifetime]
internal sealed class EscapeDiagonalQuakeState(RoomMainCarryover carryover)
{
    /// <summary>The value the following room's main inherits.</summary>
    private readonly RoomMainCarryover _carryover = Ensure.NotNull(carryover);

    /// <summary>The updates left of the diagonal quake; zero while the room shakes horizontally.</summary>
    internal WrappingCountdown Countdown
    {
        get => _carryover.Countdown;
        set => _carryover.LeaveCountdown(value);
    }
}

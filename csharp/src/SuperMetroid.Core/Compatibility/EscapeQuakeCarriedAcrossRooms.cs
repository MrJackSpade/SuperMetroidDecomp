namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// An escape room's diagonal quake carries on across rooms (#1470, #2572): the countdown of the diagonal shake is what
/// the previous room's main left, so an escape room entered while the quake runs keeps shaking diagonally for what is
/// left of it. Only the two escape setups that switch quakes clear it as the room loads; every other escape room carries
/// on what it finds, whichever main left it.
/// </summary>
/// <remarks>
/// Cleared by the two quake-switching setups once the room's enemies load, read and left by the shaking escape rooms'
/// main on every update. The value is the one the room mains share (<see cref="RoomMainCarryover"/>). Evidence: the
/// Zebes escape's shaking room mains count their diagonal quake in the room mains' shared word, which room loading
/// never clears, and only the two setups that switch the escape's quake zero it.
/// </remarks>
/// <param name="carryover">The value the room mains share.</param>
[CompatibilityService("an escape room's diagonal quake counts down from what the previous room's main left, unless one of the two quake-switching setups clears it")]
public sealed partial class EscapeQuakeCarriedAcrossRooms(RoomMainCarryover carryover)
{
    /// <summary>The value the room mains share.</summary>
    private readonly RoomMainCarryover _carryover = Ensure.NotNull(carryover);

    /// <summary>The updates left of the diagonal quake; zero while the room shakes horizontally.</summary>
    public WrappingCountdown Countdown
    {
        get => WrappingCountdown.Start(_carryover.Value);
        set => _carryover.Leave(value.RemainingFrames);
    }

    /// <summary>Ends the quake the previous room's main left, as the two quake-switching escape setups do.</summary>
    public void Clear() => _carryover.Leave(0);
}

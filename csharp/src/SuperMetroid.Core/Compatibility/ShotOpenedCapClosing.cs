namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a blue cap a shot opened closes behind Samus when she arrives through a door at its block (#1906): it never
/// does once one particular door has been opened for good, and the door's own closer is placed instead. The door is the
/// Landing Site's green door, which the game composes this rule with.
/// </summary>
/// <remarks>
/// <para>
/// Consulted when Samus arrives through a door and the first room object at the door's block, in the room's update
/// order, is a blue cap a shot opened: one still standing from before the door, as when room-object updates were suspended
/// across it. Lifetime: the room's; the room loader composes it with each room's objects. It holds no state that changes.
/// </para>
/// <para>
/// Evidence: the original's block-spawned room objects start with a cleared room argument, and the blue cap a shot opens
/// never writes one. The check that closes the cap at the arrival door's block reads every cap's room argument as the
/// number of its door and skips the closing when that door is recorded as opened for good. A cleared argument therefore
/// names door number zero, the Landing Site's green door, so a shot cap's closing depends on that door alone.
/// </para>
/// </remarks>
/// <typeparam name="TDoor">The game's door identity.</typeparam>
public sealed partial class ShotOpenedCapClosing<TDoor>
    where TDoor : class
{
    /// <summary>Composes the rule with the door whose opening for good keeps a shot-opened cap from closing.</summary>
    /// <param name="consultedDoor">The Landing Site's green door.</param>
    public ShotOpenedCapClosing(TDoor consultedDoor) => ConsultedDoor = Ensure.NotNull(consultedDoor);

    /// <summary>The door whose opening for good keeps a shot-opened cap from closing behind Samus.</summary>
    public TDoor ConsultedDoor { get; }

    /// <summary>Whether a blue cap a shot opened stays open behind Samus, given which doors are opened for good.</summary>
    /// <param name="openedForGood">Whether a door has been opened for good.</param>
    public bool StaysOpen(Func<TDoor, bool> openedForGood) => Ensure.NotNull(openedForGood)(ConsultedDoor);
}

using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A room object at a door that stays open when Samus arrives through the door, instead of closing behind her (#2091): a
/// cap or eye door whose door has been opened for good, or a blue cap a shot opened while the room's
/// <see cref="ShotOpenedCapClosing{TDoor}"/> rule keeps it open. The door transition asks it before any closing.
/// </summary>
internal interface IDoorKeptOpen
{
    /// <summary>Whether the object stays open behind Samus.</summary>
    /// <param name="openedForGood">Whether a door has been opened for good.</param>
    /// <param name="shotCapClosing">Whether a blue cap a shot opened closes behind Samus.</param>
    bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood, ShotOpenedCapClosing<LockedDoor> shotCapClosing);
}

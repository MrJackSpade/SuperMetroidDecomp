using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// An eye door's body, its middle (#1554), the part that stands in the door (#2493): when Samus arrives through the door
/// it stays open once the door has been opened for good, and otherwise closes behind her by leaving the room on its next
/// update.
/// </summary>
/// <param name="orientation">The way the door faces.</param>
/// <param name="door">The door the body's placement named.</param>
internal sealed class EyeDoorBodyObject(EyeDoorOrientation orientation, PlacedDoor door)
    : EyeDoorPartObject(new EyeDoorPart(EyeDoorBodyPart.Middle, orientation), door), IDoorBehindSamus
{
    /// <summary>A body whose door has been opened for good stays open behind Samus.</summary>
    public bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood) =>
        Door.IsOpenedForGood(openedForGood);

    /// <inheritdoc/>
    public void CloseBehindSamus() => LeaveOnNextUpdate();
}

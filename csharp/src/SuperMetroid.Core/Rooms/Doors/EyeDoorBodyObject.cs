using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// An eye door's body, its middle (#1554), the part that stands in the door (#2493): when Samus arrives through the door
/// it stays open once the door has been opened for good, and otherwise closes behind her by leaving the room on its next
/// update.
/// </summary>
internal sealed partial class EyeDoorBodyObject
    : EyeDoorPartObject, IDoorBehindSamus
{
    /// <summary>Creates an <see cref="EyeDoorBodyObject"/>.</summary>
    /// <param name="orientation">The way the door faces.</param>
    /// <param name="door">The door the body's placement named.</param>
    public EyeDoorBodyObject(EyeDoorOrientation orientation, PlacedDoor door)
        : base(new EyeDoorPart(EyeDoorBodyPart.Middle, orientation), door)
    {
    }

    /// <summary>A body whose door has been opened for good stays open behind Samus.</summary>
    public bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood) =>
        Door.IsOpenedForGood(openedForGood);

    /// <inheritdoc/>
    public void CloseBehindSamus() => LeaveOnNextUpdate();
}

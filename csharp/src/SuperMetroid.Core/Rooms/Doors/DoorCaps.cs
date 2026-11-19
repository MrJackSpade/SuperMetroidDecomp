using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of the door caps (#1296, #1554, #1994): the cleared opening, the blue, grey and coloured caps closed and
/// sliding aside, and the blue image a hit flashes, each per facing.
/// </summary>
internal static class DoorCaps
{
    /// <summary>
    /// The draw of <paramref name="image"/> of a cap painted <paramref name="paint"/> facing <paramref name="orientation"/>:
    /// the cleared opening, one of the paint's closing images, or the blue image a hit flashes.
    /// </summary>
    internal static RoomObjectDraw Image(DoorCapPaint paint, ColoredDoorOrientation orientation, DoorCapFrame image) => image switch
    {
        DoorCapFrame.Clear => GreyDoorCaps.Family.Of(GreyDoorCaps.Draw.ClearedOf(orientation)),
        DoorCapFrame.Closed => paint.Of(orientation, DoorCapLook.Closed),
        DoorCapFrame.Ajar => paint.Of(orientation, DoorCapLook.Ajar),
        DoorCapFrame.HalfOpen => paint.Of(orientation, DoorCapLook.HalfOpen),
        DoorCapFrame.NearlyOpen => paint.Of(orientation, DoorCapLook.NearlyOpen),
        DoorCapFrame.BlueFlash => DoorCapPaint.Blue.Of(orientation, DoorCapLook.Closed),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {image}."),
    };

    /// <summary>The four looks of a cap closing and sliding aside, in the order it opens.</summary>
    internal static IReadOnlyList<DoorCapLook> OpeningLooks { get; } =
        [DoorCapLook.Closed, DoorCapLook.Ajar, DoorCapLook.HalfOpen, DoorCapLook.NearlyOpen];
}

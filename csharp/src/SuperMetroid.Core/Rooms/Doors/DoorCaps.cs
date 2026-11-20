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
    internal static RoomObjectDraw Image(DoorCapPaint paint, ColoredDoorOrientation orientation, DoorCapFrame image) =>
        Images[image](paint, orientation);

    /// <summary>What each door cap image draws in a cap's paint and orientation.</summary>
    private static IdentityTable<DoorCapFrame, Func<DoorCapPaint, ColoredDoorOrientation, RoomObjectDraw>> Images { get; } = new(new Dictionary<DoorCapFrame, Func<DoorCapPaint, ColoredDoorOrientation, RoomObjectDraw>>
    {
        [DoorCapFrame.Clear] = static (_, orientation) => GreyDoorCaps.Family.Of(GreyDoorCaps.Draw.ClearedOf(orientation)),
        [DoorCapFrame.Closed] = static (paint, orientation) => paint.Of(orientation, DoorCapLook.Closed),
        [DoorCapFrame.Ajar] = static (paint, orientation) => paint.Of(orientation, DoorCapLook.Ajar),
        [DoorCapFrame.HalfOpen] = static (paint, orientation) => paint.Of(orientation, DoorCapLook.HalfOpen),
        [DoorCapFrame.NearlyOpen] = static (paint, orientation) => paint.Of(orientation, DoorCapLook.NearlyOpen),
        [DoorCapFrame.BlueFlash] = static (_, orientation) => DoorCapPaint.Blue.Of(orientation, DoorCapLook.Closed),
    });

    /// <summary>The four looks of a cap closing and sliding aside, in the order it opens.</summary>
    internal static IReadOnlyList<DoorCapLook> OpeningLooks { get; } =
        [DoorCapLook.Closed, DoorCapLook.Ajar, DoorCapLook.HalfOpen, DoorCapLook.NearlyOpen];
}

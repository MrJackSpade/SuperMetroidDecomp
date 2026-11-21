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
    /// the look it shows in the paint it is drawn in: the cleared opening in the grey caps', one of the paint's closing
    /// images in its own, or the blue cap a hit flashes in the blue paint's.
    /// </summary>
    internal static RoomObjectDraw Image(DoorCapPaint paint, ColoredDoorOrientation orientation, DoorCapShown image) => (image.Paint switch
    {
        DoorCapShownPaint.Own => paint,
        DoorCapShownPaint.Grey => DoorCapPaint.Grey,
        DoorCapShownPaint.Blue => DoorCapPaint.Blue,
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapShownPaint)} {image.Paint}."),
    }).Of(orientation, image.Look);

    /// <summary>The four looks of a cap closing and sliding aside, in the order it opens.</summary>
    internal static IReadOnlyList<DoorCapLook> OpeningLooks { get; } =
        [DoorCapLook.Closed, DoorCapLook.Ajar, DoorCapLook.HalfOpen, DoorCapLook.NearlyOpen];
}

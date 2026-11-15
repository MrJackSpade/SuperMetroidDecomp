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
        DoorCapFrame.Closed => Painted(paint, orientation, DoorCapLook.Closed),
        DoorCapFrame.Ajar => Painted(paint, orientation, DoorCapLook.Ajar),
        DoorCapFrame.HalfOpen => Painted(paint, orientation, DoorCapLook.HalfOpen),
        DoorCapFrame.NearlyOpen => Painted(paint, orientation, DoorCapLook.NearlyOpen),
        DoorCapFrame.BlueFlash => Painted(DoorCapPaint.Blue, orientation, DoorCapLook.Closed),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {image}."),
    };

    /// <summary>The paint of a <paramref name="color"/> cap.</summary>
    internal static DoorCapPaint PaintOf(ColoredDoorColor color) => color switch
    {
        ColoredDoorColor.Yellow => DoorCapPaint.Yellow,
        ColoredDoorColor.Green => DoorCapPaint.Green,
        ColoredDoorColor.Red => DoorCapPaint.Red,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {color}."),
    };

    /// <summary>The <paramref name="paint"/> cap facing <paramref name="orientation"/> showing <paramref name="look"/>.</summary>
    private static RoomObjectDraw Painted(DoorCapPaint paint, ColoredDoorOrientation orientation, DoorCapLook look) => paint switch
    {
        DoorCapPaint.Blue => BlueDoorCaps.Family.Of(new BlueDoorCaps.Draw(orientation, look)),
        DoorCapPaint.Grey => GreyDoorCaps.Family.Of(new GreyDoorCaps.Draw(orientation, look)),
        DoorCapPaint.Yellow => ColoredDoorCaps.Family.Of(new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, orientation, look)),
        DoorCapPaint.Green => ColoredDoorCaps.Family.Of(new ColoredDoorCaps.Draw(ColoredDoorColor.Green, orientation, look)),
        DoorCapPaint.Red => ColoredDoorCaps.Family.Of(new ColoredDoorCaps.Draw(ColoredDoorColor.Red, orientation, look)),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapPaint)} {paint}."),
    };

    /// <summary>The four looks of a cap closing and sliding aside, in the order it opens.</summary>
    internal static IReadOnlyList<DoorCapLook> OpeningLooks { get; } =
        [DoorCapLook.Closed, DoorCapLook.Ajar, DoorCapLook.HalfOpen, DoorCapLook.NearlyOpen];

    /// <summary>Whether <paramref name="look"/> is the cap closed or sliding aside, the looks every painted cap draws.</summary>
    internal static bool IsOpeningLook(DoorCapLook look) =>
        look is DoorCapLook.Closed or DoorCapLook.Ajar or DoorCapLook.HalfOpen or DoorCapLook.NearlyOpen;
}

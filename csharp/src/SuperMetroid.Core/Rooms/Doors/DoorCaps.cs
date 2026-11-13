using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of the door caps (#1296, #1554): the cleared opening, the blue, grey and coloured caps through their four
/// closing images, and the blue image a hit flashes, each per facing.
/// </summary>
internal static class DoorCaps
{
    /// <summary>
    /// The draw of <paramref name="image"/> of a cap painted <paramref name="paint"/> facing <paramref name="orientation"/>:
    /// the cleared opening, one of the paint's closing images, or the blue image a hit flashes.
    /// </summary>
    internal static RoomObjectDraw Image(DoorCapPaint paint, ColoredDoorOrientation orientation, DoorCapFrame image) => image switch
    {
        DoorCapFrame.Clear => RoomObjectDraw.Of(GreyDoorCaps.Draw.ClearedOf(orientation)),
        DoorCapFrame.Closed0 => Closed(paint, orientation, 0),
        DoorCapFrame.Closed1 => Closed(paint, orientation, 1),
        DoorCapFrame.Closed2 => Closed(paint, orientation, 2),
        DoorCapFrame.Closed3 => Closed(paint, orientation, 3),
        DoorCapFrame.BlueFlash => Closed(DoorCapPaint.Blue, orientation, 0),
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

    /// <summary>The <paramref name="paint"/> cap facing <paramref name="orientation"/> at closing image <paramref name="closingImage"/>.</summary>
    private static RoomObjectDraw Closed(DoorCapPaint paint, ColoredDoorOrientation orientation, int closingImage) => paint switch
    {
        DoorCapPaint.Blue => RoomObjectDraw.Of(new BlueDoorCaps.Draw(orientation, closingImage)),
        DoorCapPaint.Grey => RoomObjectDraw.Of(new GreyDoorCaps.Draw(orientation, closingImage)),
        DoorCapPaint.Yellow => RoomObjectDraw.Of(new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, orientation, closingImage)),
        DoorCapPaint.Green => RoomObjectDraw.Of(new ColoredDoorCaps.Draw(ColoredDoorColor.Green, orientation, closingImage)),
        DoorCapPaint.Red => RoomObjectDraw.Of(new ColoredDoorCaps.Draw(ColoredDoorColor.Red, orientation, closingImage)),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapPaint)} {paint}."),
    };
}

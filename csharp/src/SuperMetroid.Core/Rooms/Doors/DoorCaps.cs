using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of the door caps (#1296): the cleared opening, the grey and coloured caps through their four closing
/// images, and the blue cap, each per facing.
/// </summary>
internal static class DoorCaps
{
    /// <summary>The cleared opening facing <paramref name="orientation"/>.</summary>
    internal static RoomObjectDraw Clear(ColoredDoorOrientation orientation) =>
        RoomObjectDraw.Of(new GreyDoorCaps.Draw(orientation, GreyDoorObjectDrawDefinitions.ClearFrame));

    /// <summary>The grey cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static RoomObjectDraw Grey(ColoredDoorOrientation orientation, int image) =>
        RoomObjectDraw.Of(new GreyDoorCaps.Draw(orientation, Image(image)));

    /// <summary>The <paramref name="color"/> cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static RoomObjectDraw Colored(ColoredDoorColor color, ColoredDoorOrientation orientation, int image) =>
        RoomObjectDraw.Of(new ColoredDoorCaps.Draw(color, orientation, Image(image)));

    /// <summary>The blue cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static RoomObjectDraw Blue(ColoredDoorOrientation orientation, int image) =>
        RoomObjectDraw.Of(new BlueDoorCaps.Draw(orientation, Image(image)));

    /// <summary>Validates a closing image index.</summary>
    private static int Image(int image) =>
        (uint)image < DoorObjectDrawListDefinitions.CapImages ? image : throw new ArgumentOutOfRangeException(nameof(image));
}

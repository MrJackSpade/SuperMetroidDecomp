using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of the door caps (#1296): the cleared opening, the grey and coloured caps through their four closing
/// images, and the blue cap, each per facing.
/// </summary>
internal static class DoorPlmDrawListDefinitions
{
    /// <summary>A cap's closing images, fully closed (0) to almost open (3).</summary>
    private const int CapImages = 4;

    /// <summary>The cleared opening facing <paramref name="orientation"/>.</summary>
    internal static PlmDraw Clear(ColoredDoorOrientation orientation) =>
        PlmDraw.Of(new GreyDoorPlmDrawDefinitions.Draw(orientation, GreyDoorPlmDrawDefinitions.ClearFrame));

    /// <summary>The grey cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static PlmDraw Grey(ColoredDoorOrientation orientation, int image) =>
        PlmDraw.Of(new GreyDoorPlmDrawDefinitions.Draw(orientation, Image(image)));

    /// <summary>The <paramref name="color"/> cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static PlmDraw Colored(ColoredDoorColor color, ColoredDoorOrientation orientation, int image) =>
        PlmDraw.Of(new ColoredDoorPlmDrawDefinitions.Draw(color, orientation, Image(image)));

    /// <summary>The blue cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static PlmDraw Blue(ColoredDoorOrientation orientation, int image) =>
        PlmDraw.Of(new BlueDoorPlmDrawDefinitions.Draw(orientation, Image(image)));

    /// <summary>Validates a closing image index.</summary>
    private static int Image(int image) =>
        (uint)image < CapImages ? image : throw new ArgumentOutOfRangeException(nameof(image));
}

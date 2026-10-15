namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The bank-$84 draw lists of the door caps (#1296): the cleared opening, the grey and coloured caps through their
/// four closing images, and the blue cap, each per facing. Lists of one kind sit at fixed strides.
/// </summary>
internal static class DoorPlmDrawListDefinitions
{
    /// <summary>The left-facing cleared opening ($84:A677).</summary>
    private const ushort ClearLeft = 0xa677;
    /// <summary>The left-facing grey cap ($84:A6A7).</summary>
    private const ushort GreyLeft = 0xa6a7;
    /// <summary>The left-facing yellow cap ($84:A767); green and red follow.</summary>
    private const ushort ColoredYellowLeft = 0xa767;
    /// <summary>The left-facing blue cap ($84:A9B3).</summary>
    private const ushort BlueLeft = 0xa9b3;
    /// <summary>The bytes between successive images of one cap, and between the cleared openings of each facing.</summary>
    private const int ListBytes = 12;
    /// <summary>The bytes between one facing's grey or coloured cap and the next facing's.</summary>
    private const int CapFacingBytes = 48;
    /// <summary>The bytes between one colour's caps and the next colour's.</summary>
    private const int ColorBytes = 192;
    /// <summary>The bytes between one facing's blue cap and the next facing's.</summary>
    private const int BlueFacingBytes = 60;
    /// <summary>A cap's closing images, fully closed (0) to almost open (3).</summary>
    private const int CapImages = 4;

    /// <summary>The cleared opening facing <paramref name="orientation"/>.</summary>
    internal static ushort Clear(ColoredDoorOrientation orientation) =>
        checked((ushort)(ClearLeft + (int)orientation * ListBytes));

    /// <summary>The grey cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static ushort Grey(ColoredDoorOrientation orientation, int image) =>
        checked((ushort)(GreyLeft + (int)orientation * CapFacingBytes + Image(image) * ListBytes));

    /// <summary>The <paramref name="color"/> cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static ushort Colored(ColoredDoorColor color, ColoredDoorOrientation orientation, int image) =>
        checked((ushort)(ColoredYellowLeft + (int)color * ColorBytes + (int)orientation * CapFacingBytes + Image(image) * ListBytes));

    /// <summary>The blue cap facing <paramref name="orientation"/> at closing image <paramref name="image"/>.</summary>
    internal static ushort Blue(ColoredDoorOrientation orientation, int image) =>
        checked((ushort)(BlueLeft + (int)orientation * BlueFacingBytes + Image(image) * ListBytes));

    /// <summary>Validates a closing image index.</summary>
    private static int Image(int image) =>
        (uint)image < CapImages ? image : throw new ArgumentOutOfRangeException(nameof(image));
}

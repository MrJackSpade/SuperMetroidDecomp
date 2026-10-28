namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The shape shared by the door caps' draws (#1296): the cleared opening, the grey and coloured caps through their four
/// closing images, and the blue cap, each per facing.
/// </summary>
internal static class DoorPlmDrawListDefinitions
{
    /// <summary>A cap's closing images, fully closed (0) to almost open (3).</summary>
    internal const int CapImages = 4;

    /// <summary>A cap's native run header facing left or right: four blocks with the vertical direction bit.</summary>
    internal const int VerticalCapRun = 0x8004;

    /// <summary>A cap's native run header facing up or down: four blocks laid out horizontally.</summary>
    internal const int HorizontalCapRun = 4;
}

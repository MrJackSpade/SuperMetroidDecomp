namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The shape shared by the door caps' draws (#1296): the cleared opening, the grey and coloured caps through their four
/// closing images, and the blue cap, each per facing.
/// </summary>
internal static class DoorPlmDrawListDefinitions
{
    /// <summary>A cap's closing images, fully closed (0) to almost open (3).</summary>
    internal const int CapImages = 4;
}

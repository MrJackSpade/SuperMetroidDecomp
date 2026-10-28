using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge camera configuration owned by the living Kraid encounter.</summary>
internal static class KraidCameraDefinitions
{
    /// <summary>Four row-major screens written by the two native word stores at $A7:A9ED/A9F4.</summary>
    public const int ScreenCount = 4;

    /// <summary>Scrolls[0..3] written at $A7:A9EA-A9F4: only the lower-left screen is open.</summary>
    public static readonly RoomScrollState[] InitialScrolls =
        [RoomScrollState.RedBoundary, RoomScrollState.RedBoundary, RoomScrollState.Blue, RoomScrollState.RedBoundary];

    /// <summary>Scrolls[0..3] written by Kraid_GetsBig_ReleaseCamera at $A7:C0A1.</summary>
    /// <remarks>The two-column room permits vertical scrolling on the upper row only.</remarks>
    public static readonly RoomScrollState[] GrownScrolls =
        [RoomScrollState.Green, RoomScrollState.Green, RoomScrollState.Blue, RoomScrollState.Blue];
}

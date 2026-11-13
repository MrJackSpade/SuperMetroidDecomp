using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Camera configuration owned by the living Kraid encounter.</summary>
internal static class KraidCameraDefinitions
{
    /// <summary>Four row-major screens, written two at a time.</summary>
    public const int ScreenCount = 4;

    /// <summary>Scrolls[0..3] written: only the lower-left screen is open.</summary>
    public static readonly RoomScrollState[] InitialScrolls =
        [RoomScrollState.RedBoundary, RoomScrollState.RedBoundary, RoomScrollState.Blue, RoomScrollState.RedBoundary];

    /// <summary>Scrolls[0..3] written as Kraid grows and releases the camera.</summary>
    /// <remarks>The two-column room permits vertical scrolling on the upper row only.</remarks>
    public static readonly RoomScrollState[] GrownScrolls =
        [RoomScrollState.Green, RoomScrollState.Green, RoomScrollState.Blue, RoomScrollState.Blue];
}

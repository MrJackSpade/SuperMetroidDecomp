using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The scroll state each screen of Kraid's room, two columns by two rows, takes before and after Kraid grows.</summary>
internal static class KraidCameraDefinitions
{
    /// <summary>The room's four screens as Kraid's setup writes them, row by row: only the lower-left screen is open.</summary>
    public static readonly ScreenScrollChange[] InitialScrolls =
    [
        new(new(0, 0), RoomScrollState.Closed),
        new(new(1, 0), RoomScrollState.Closed),
        new(new(0, 1), RoomScrollState.OpenAlignedToTop),
        new(new(1, 1), RoomScrollState.Closed),
    ];

    /// <summary>The room's four screens as Kraid grows and releases the camera, row by row.</summary>
    /// <remarks>The two-column room permits vertical scrolling on the upper row only.</remarks>
    public static readonly ScreenScrollChange[] GrownScrolls =
    [
        new(new(0, 0), RoomScrollState.Open),
        new(new(1, 0), RoomScrollState.Open),
        new(new(0, 1), RoomScrollState.OpenAlignedToTop),
        new(new(1, 1), RoomScrollState.OpenAlignedToTop),
    ];
}

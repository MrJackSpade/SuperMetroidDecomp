using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The scroll state of each of Kraid's room screens, before and after Kraid grows.</summary>
internal static class KraidCamera
{
    /// <summary>Scrolls[0..3] written at $A7:A9EA-A9F4: only the lower-left screen is open.</summary>
    public static RoomScrollState InitialScroll(int screen) => KraidCameraDefinitions.InitialScrolls[CheckScreen(screen)];

    /// <summary>Scrolls[0..3] written by Kraid_GetsBig_ReleaseCamera at $A7:C0A1.</summary>
    public static RoomScrollState GrownScroll(int screen) => KraidCameraDefinitions.GrownScrolls[CheckScreen(screen)];

    /// <summary>Rejects a screen outside the room's four.</summary>
    private static int CheckScreen(int screen) =>
        (uint)screen < KraidCameraDefinitions.ScreenCount ? screen : throw new ArgumentOutOfRangeException(nameof(screen));
}

namespace SuperMetroid.Core.Game;

/// <summary>Cartridge camera configuration owned by the living Kraid encounter.</summary>
internal static class KraidCameraDefinitions
{
    /// <summary>CameraDistanceIndex written by InitAI_Kraid at $A7:A9E4-A9E7.</summary>
    public const ushort CameraDistanceIndex = 2;

    /// <summary>Scrolls[0..3] written at $A7:A9EA-A9F4: only the lower-left screen is open.</summary>
    public static RoomScrollState InitialScroll(int screen)
    {
        if ((uint)screen >= ScreenCount) throw new IndexOutOfRangeException();
        return screen == 2 ? RoomScrollState.Blue : RoomScrollState.RedBoundary;
    }

    /// <summary>Scrolls[0..3] written by Kraid_GetsBig_ReleaseCamera at $A7:C0A1.</summary>
    /// <remarks>The two-column room permits vertical scrolling on the upper row only.</remarks>
    public static RoomScrollState GrownScroll(int screen)
    {
        if ((uint)screen >= ScreenCount) throw new IndexOutOfRangeException();
        return screen < 2 ? RoomScrollState.Green : RoomScrollState.Blue;
    }

    /// <summary>Four row-major screens written by the two native word stores at $A7:A9ED/A9F4.</summary>
    public const int ScreenCount = 4;
}

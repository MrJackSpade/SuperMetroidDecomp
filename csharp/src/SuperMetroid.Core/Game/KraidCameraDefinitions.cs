namespace SuperMetroid.Core.Game;

/// <summary>Cartridge camera configuration owned by the living Kraid encounter.</summary>
internal static class KraidCameraDefinitions
{
    /// <summary>CameraDistanceIndex written by InitAI_Kraid at $A7:A9E4-A9E7.</summary>
    public const ushort CameraDistanceIndex = 2;

    /// <summary>Scrolls[0..3] written at $A7:A9EA-A9F4: only the lower-left screen is open.</summary>
    public static ReadOnlySpan<RoomScrollState> InitialScrolls =>
        [RoomScrollState.RedBoundary, RoomScrollState.RedBoundary,
         RoomScrollState.Blue, RoomScrollState.RedBoundary];

    /// <summary>Scrolls[0..3] written by Kraid_GetsBig_ReleaseCamera at $A7:C0A1.</summary>
    public static ReadOnlySpan<RoomScrollState> GrownScrolls =>
        [RoomScrollState.Green, RoomScrollState.Green, RoomScrollState.Blue, RoomScrollState.Blue];
}

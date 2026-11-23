namespace SuperMetroid.Core.Areas.Brinstar;

/// <summary>
/// When the Dachora room's main moves the camera toward the shaft that drops from the room's top corridor (#2293): only
/// while the shaft's top screen stands open, and only while the camera's top-left lies above and left of the bounds here.
/// </summary>
internal static partial class DachoraShaftCameraDefinitions
{
    /// <summary>The screen whose opening lets the camera move: the shaft's top screen, below the corridor's fifth screen.</summary>
    internal static readonly Position16<RoomScreens> ShaftTopScreen = new(4, 1);

    /// <summary>The room Y the camera's top must lie above for it to move: the top of the room's sixth screen row.</summary>
    internal const int CameraAboveY = 1280;

    /// <summary>The room X the camera's left edge must lie left of for it to move: halfway across the room's fourth screen column.</summary>
    internal const int CameraLeftOfX = 896;

    /// <summary>How far right the camera moves on each update it moves, in whole pixels.</summary>
    internal const int StepPixels = 3;
}

using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>The room's layer-2 plane shown below the Ceres Mode 7 floor, after a bottom-of-screen video-mode switch.</summary>
/// <param name="FirstScanline">Inclusive physical screen Y where the lower band begins; sampling does not restart its scroll origin here.</param>
/// <param name="Scroll">BG2's scroll in pixels.</param>
/// <param name="MapSize">BG2 size in eight-pixel cells, 32 or 64 on each axis when used by a gameplay layer.</param>
/// <param name="Room">The room's layer-2 plane the band shows (#1346, #1403), drawn from its block pictures.</param>
public readonly record struct TurningRoomLowerBand(int FirstScanline, Position16<BackgroundPixels> Scroll, Extent16<TilemapCells> MapSize,
    RoomBackgroundLayer Room);

using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>The room's background shown in a band below the turning Ceres floor, at the bottom of the screen.</summary>
/// <param name="FirstLine">The first screen line of the band; the background's scroll is measured from the top of the screen, not from here.</param>
/// <param name="Scroll">The background's scroll in pixels.</param>
/// <param name="MapSize">The background's map size in eight-pixel cells, 32 or 64 on each axis when a gameplay layer shows it.</param>
/// <param name="Room">The room's layer-2 plane the band shows (#1346, #1403), drawn from its block pictures.</param>
public readonly record struct TurningRoomLowerBand(int FirstLine, Position16<BackgroundPixels> Scroll, Extent16<TilemapCells> MapSize,
    RoomBackgroundLayer Room);

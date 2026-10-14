namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Source-room PPU scroll words retained until directional door setup derives its offsets.
/// </summary>
internal readonly record struct DoorOpeningPpuScroll(
    Position16<RoomPixels> Bg1Scroll);

using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Source-room PPU scroll words retained until directional door setup derives its offsets, and the source room's BG1
/// the opening scroll keeps showing.
/// </summary>
/// <param name="Bg1Scroll">The source room's BG1 scroll registers.</param>
/// <param name="SourceBackground">The source room's BG1 with its scroll offset.</param>
internal readonly record struct DoorOpeningPpuScroll(
    Position16<RoomPixels> Bg1Scroll, RoomBackgroundLayer SourceBackground);

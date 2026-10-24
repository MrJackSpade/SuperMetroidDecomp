using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What the source room showed when the door opened: its foreground camera scroll, kept until the directional door
/// setup derives its offsets, and its foreground layer, which the opening scroll keeps showing.
/// </summary>
/// <param name="Bg1Scroll">The source room's foreground camera scroll.</param>
/// <param name="SourceBackground">The source room's foreground layer with its scroll offset.</param>
internal readonly record struct DoorOpeningSourceView(
    Position16<RoomPixels> Bg1Scroll, RoomBackgroundLayer SourceBackground);

using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What the source room showed when the door opened: its foreground view, kept until the directional door setup places
/// the source screen on the destination, and its foreground layer, which the opening scroll keeps showing.
/// </summary>
/// <param name="ForegroundScroll">The source room's foreground view.</param>
/// <param name="SourceBackground">The source room's foreground layer.</param>
internal readonly record struct DoorOpeningSourceView(
    Position16<RoomPixels> ForegroundScroll, RoomBackgroundLayer SourceBackground);

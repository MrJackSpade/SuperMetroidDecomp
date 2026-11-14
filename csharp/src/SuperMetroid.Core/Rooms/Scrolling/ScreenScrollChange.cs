namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// One screen a door setup, a scroll trigger or a boss changes (#1741): the screen, by its column and row in the room (for
/// a door setup or a scroll trigger, a row past the room's rows reaches the kept cells after them), and the scroll state it
/// gives that screen (closed to the camera, open, or open with the camera aligned to the screen's top).
/// </summary>
/// <param name="Screen">The screen's column and row in the room.</param>
/// <param name="State">The scroll state the screen takes.</param>
public readonly record struct ScreenScrollChange(Position16<RoomScreens> Screen, RoomScrollState State);

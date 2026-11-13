using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One screen a door setup closes or opens (#1741): the screen, by its column and row in the room its doors lead into (a
/// row past the room's rows reaches the kept cells after them), and the scroll state it gives that screen (closed to the
/// camera, open, or open with the camera aligned to the screen's top).
/// </summary>
/// <param name="Screen">The screen's column and row in the room.</param>
/// <param name="State">The scroll state the setup gives it.</param>
internal readonly record struct DoorScreenChange(Position16<RoomScreens> Screen, RoomScrollState State);

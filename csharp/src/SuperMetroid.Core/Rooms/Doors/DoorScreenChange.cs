using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One screen a door setup closes or opens: the screen, by its row-major cell number in the room's scroll grid, and the
/// scroll state it gives that screen (closed to the camera, open, or open with the camera aligned to the screen's top).
/// </summary>
internal readonly record struct DoorScreenChange(int Screen, RoomScrollState State);

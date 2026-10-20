using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One write of a door scroll callback: the scroll cell, by row-major index, and its new state.</summary>
internal readonly record struct DoorScrollWrite(int ScrollIndex, RoomScrollState State);
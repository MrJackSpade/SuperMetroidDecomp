using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One write of a door scroll callback: the scroll cell, by row-major index, and its new state.</summary>
internal readonly record struct DoorScrollWrite(int ScrollIndex, RoomScrollState State);
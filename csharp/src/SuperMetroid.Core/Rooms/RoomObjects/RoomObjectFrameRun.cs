namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The one run of a room object's authored frames (#2480): a timed terrain change's, a block reaction's or the elevator
/// platform's loop, each frame shown for its own updates, played through the shared animation player
/// (<see cref="RoomObjectFrameRuns"/>).
/// </summary>
internal enum RoomObjectFrameRun
{
    /// <summary>The frames in the order they draw, each for its own updates.</summary>
    Frames,
}

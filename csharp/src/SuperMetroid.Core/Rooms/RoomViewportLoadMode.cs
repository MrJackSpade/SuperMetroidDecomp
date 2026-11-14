namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Selects how the room's initial background is published for a room load.
/// </summary>
internal enum RoomViewportLoadMode
{
    /// <summary>
    /// Load stations, startup, and explicit debug loads show the destination's whole view at once.
    /// </summary>
    DisplayInitialViewport,

    /// <summary>
    /// Door loads keep showing the source room's screen while the door's scroll moves the destination in around it.
    /// </summary>
    StreamThroughDoor,
}

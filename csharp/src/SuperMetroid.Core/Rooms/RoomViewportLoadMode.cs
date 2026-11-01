namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Selects how the room's initial background is published for a room load.
/// </summary>
internal enum RoomViewportLoadMode
{
    /// <summary>
    /// Load stations, startup, and explicit debug loads draw all
    /// seventeen visible block columns immediately.
    /// </summary>
    DisplayInitialViewport,

    /// <summary>
    /// Door loads retain the source room's background and replace it one boundary
    /// row or column at a time as the door scrolls.
    /// </summary>
    StreamThroughDoor,
}

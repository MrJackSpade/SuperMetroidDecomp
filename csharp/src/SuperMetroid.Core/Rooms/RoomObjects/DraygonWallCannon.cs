namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Draygon's four wall cannons. Destroying a cannon's room object stops Draygon firing from it; Draygon's AI skips a disabled
/// cannon when it fires. The room import decodes each cannon placement's room-object argument into the cannon it controls.
/// </summary>
public enum DraygonWallCannon
{
    /// <summary>The upper-left cannon, placed already destroyed.</summary>
    UpperLeft,
    /// <summary>The lower-left cannon.</summary>
    LowerLeft,
    /// <summary>The upper-right cannon.</summary>
    UpperRight,
    /// <summary>The lower-right cannon.</summary>
    LowerRight,
}

namespace SuperMetroid.Core.Rooms;

/// <summary>How a door transition into or out of a room draws the screen while it scrolls.</summary>
public enum RoomDoorTransitionStyle
{
    /// <summary>The transition scrolls the room's backgrounds with the objects.</summary>
    Standard,

    /// <summary>
    /// The transition hides BG1 below the HUD and shows the objects alone, as it does when either adjacent room asks
    /// for it.
    /// </summary>
    ObjectsOnly,
}

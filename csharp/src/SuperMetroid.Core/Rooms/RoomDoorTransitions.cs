namespace SuperMetroid.Core.Rooms;

/// <summary>How a room's door transitions show it.</summary>
public static class RoomDoorTransitions
{
    /// <summary>The door-transition style a room's definition names.</summary>
    extension(RoomHeaderDefinition room)
    {
        /// <summary>Whether door transitions into or out of this room hide the foreground below the HUD and show the objects alone.</summary>
        public bool UsesIsolatedDoorTransition => room.DoorTransition == RoomDoorTransitionStyle.ObjectsOnly;
    }
}

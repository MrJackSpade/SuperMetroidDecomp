namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>How the door of Ceres Ridley's room shows (#2545, #2573), which the door's animation changes.</summary>
internal enum CeresRidleyRoomDoorLook
{
    /// <summary>The door shows its actor frames through the room's enemies, as every other door does.</summary>
    Actor,

    /// <summary>The door is hidden as an actor and shows its fixed overlay picture at its place instead.</summary>
    Overlay,
}

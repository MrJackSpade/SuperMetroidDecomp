namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>How a Ceres door shows (#2545, #2573), which only the animations of the door of Ceres Ridley's room change.</summary>
internal enum CeresRidleyRoomDoorLook
{
    /// <summary>The door shows its actor frames through the room's enemies, as every other door does.</summary>
    Actor,

    /// <summary>The door is hidden as an actor and shows its fixed overlay picture at its place instead.</summary>
    Overlay,
}

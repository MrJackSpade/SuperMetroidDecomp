namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Who draws the door of Ceres Ridley's room (#2545), which the door's animation hands over and takes back.</summary>
internal enum CeresRidleyRoomDoorDrawing
{
    /// <summary>The door draws itself through the room's enemies, as every other door does.</summary>
    ItsOwn,

    /// <summary>
    /// The door has handed its drawing to Ridley's scene, which draws the room's door after the Baby while the door's own
    /// drawing stays hidden, until the door's animation takes it back.
    /// </summary>
    ByRidleysScene,
}

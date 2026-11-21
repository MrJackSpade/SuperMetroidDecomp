using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>An eye's request to fire its attack from the shared enemy-projectile pool.</summary>
/// <param name="Facing">The side the spawning eye faces, which places the attack.</param>
/// <param name="RoomObjectBlock">The room block of the spawning eye door.</param>
/// <param name="Door">The spawning eye door's door, which the attack keeps to burst once the door has been opened.</param>
public readonly partial record struct EyeDoorAttackRequest(
    EyeDoorOrientation Facing,
    Position16<RoomBlocks> RoomObjectBlock,
    LockedDoor Door);

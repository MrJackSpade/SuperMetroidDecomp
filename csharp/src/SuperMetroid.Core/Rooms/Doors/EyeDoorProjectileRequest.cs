using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One eye-door request to allocate an actor from the shared enemy-projectile pool.</summary>
/// <param name="Definition">What is spawned; allocation and actor lifetime belong to the enemy-projectile owner.</param>
/// <param name="Facing">The side the spawning eye faces: it places the attack and launches the sweat; smoke ignores it.</param>
/// <param name="RoomObjectBlock">The room block of the spawning eye door.</param>
/// <param name="Door">The spawning eye door's door, which attack initialization retains to burst once the door has been opened;
/// null once the door has opened, when only sweat and smoke, which do not consume it, are spawned.</param>
public readonly record struct EyeDoorProjectileRequest(
    EyeDoorProjectileDefinition Definition,
    EyeDoorOrientation Facing,
    Position16<RoomBlocks> RoomObjectBlock,
    LockedDoor? Door);

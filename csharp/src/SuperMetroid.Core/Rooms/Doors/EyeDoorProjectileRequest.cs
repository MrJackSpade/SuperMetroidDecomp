namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One eye-door request to allocate a sweat or smoke actor from the shared enemy-projectile pool; the eye's attack is an
/// <see cref="EyeDoorAttackRequest"/>.
/// </summary>
/// <param name="Definition">What is spawned; allocation and actor lifetime belong to the enemy-projectile owner.</param>
/// <param name="Facing">The side the spawning eye faces: it launches the sweat; smoke ignores it.</param>
/// <param name="RoomObjectBlock">The room block of the spawning eye door.</param>
public readonly record struct EyeDoorProjectileRequest(
    EyeDoorProjectileDefinition Definition,
    EyeDoorOrientation Facing,
    Position16<RoomBlocks> RoomObjectBlock);

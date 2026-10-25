namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One eye-door request to allocate an actor from the shared enemy-projectile pool.</summary>
/// <param name="Definition">What is spawned; allocation and actor lifetime belong to the enemy-projectile owner.</param>
/// <param name="Facing">The side the spawning eye faces: it places the attack and launches the sweat; smoke ignores it.</param>
/// <param name="PlmBlockIndex">Zero-based row-major room-cell index of the spawning PLM, in 16-pixel blocks rather than the native level-word byte offset; the consumer supplies the room row stride.</param>
/// <param name="DoorBit">Raw spawning PLM room-argument word; attack initialization retains it for the persistent opened-door-bit check, while sweat and smoke do not consume it.</param>
public readonly record struct EyeDoorProjectileRequest(
    EyeDoorProjectileDefinition Definition,
    EyeDoorOrientation Facing,
    int PlmBlockIndex,
    int DoorBit)
{
    /// <summary>The spawning PLM's room argument, 0 through 65535.</summary>
    public int DoorBit { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, DoorBit);
}

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One eye-door request to allocate an actor from the shared enemy-projectile pool.</summary>
/// <param name="Definition">What is spawned; allocation and actor lifetime belong to the enemy-projectile owner.</param>
/// <param name="Facing">The side the spawning eye faces: it places the attack and launches the sweat; smoke ignores it.</param>
/// <param name="PlmBlockIndex">Zero-based row-major room-cell index of the spawning PLM, in 16-pixel blocks rather than the native level-word byte offset; the consumer supplies the room row stride.</param>
/// <param name="DoorBit">The spawning eye door's door bit, which attack initialization retains for the persistent opened-door-bit
/// check; null once the door has opened, when only sweat and smoke, which do not consume it, are spawned.</param>
public readonly record struct EyeDoorProjectileRequest(
    EyeDoorProjectileDefinition Definition,
    EyeDoorOrientation Facing,
    int PlmBlockIndex,
    int? DoorBit)
{
    /// <summary>The spawning eye door's door bit, 0 through 65535, or null once the door has opened.</summary>
    public int? DoorBit { get; init; } = DoorBit is { } bit ? Ensure.BetweenInclusive(0, ushort.MaxValue, bit) : null;
}


namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One bank-$84 request to allocate an actor from bank $86's shared projectile pool.</summary>
/// <param name="Definition">Bank-$86 definition word: attack $B743, sweat $B751, or shared PLM dust/smoke $E517; allocation and actor lifetime belong to the enemy-projectile owner.</param>
/// <param name="Parameter">Definition-specific native operand: an even byte offset into attack-origin or sweat-velocity words, or packed dust/smoke animation and placement flags.</param>
/// <param name="PlmBlockIndex">Zero-based row-major room-cell index of the spawning PLM, in 16-pixel blocks rather than the native level-word byte offset; the consumer supplies the room row stride.</param>
/// <param name="DoorBit">Raw spawning PLM room-argument word; attack initialization retains it for the persistent opened-door-bit check, while sweat and smoke do not consume it.</param>
public readonly record struct EyeDoorProjectileRequest(
    EyeDoorProjectileDefinition Definition,
    int Parameter,
    int PlmBlockIndex,
    int DoorBit)
{
    /// <summary>The definition-specific operand, 0 through 65535.</summary>
    public int Parameter { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Parameter);

    /// <summary>The spawning PLM's room argument, 0 through 65535.</summary>
    public int DoorBit { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, DoorBit);
}

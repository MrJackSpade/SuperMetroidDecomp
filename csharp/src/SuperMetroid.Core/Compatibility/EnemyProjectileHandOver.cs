namespace SuperMetroid.Core.Compatibility;

/// <summary>What one enemy projectile place hands the next room (<see cref="RoomHandOver"/>).</summary>
/// <param name="Position">The place's whole position; its sub-pixel part does not survive.</param>
/// <param name="Velocity">The place's velocity.</param>
[CompatibilityService("an enemy projectile that does not set its position or velocity starts from what its place held in the room before")]
public readonly record struct EnemyProjectileHandOver(Position16<RoomPixels> Position, EightEightVector<RoomPixels> Velocity);

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What an enemy projectile leaves in its enemy-projectile place when it ends: its whole position and its velocity
/// (#1476, #1590). The next projectile spawned into the place starts from them, in the same room or, through
/// <see cref="RoomHandOver"/>, in the next; a spawn that does not set its position or velocity itself keeps them, so
/// where and how fast the place's last projectile was decides where and how fast the new one starts. Its sub-pixel
/// position does not survive, and nothing else the previous projectile held reaches the new one.
/// </summary>
/// <remarks>
/// Read when a spawn takes the place, from the place's previous projectile as it stands then (after any movement its
/// update made once it had freed the place), and at the room load's teardown for the next room. Evidence: the original
/// spawn clears a slot's sub-positions and the values its initializers write, and leaves its whole positions and
/// velocities; deletion clears only the slot's identity. The 13%, 100%, lowc and Ridley movies diverge in enemy
/// projectiles without the values carried across rooms (db3aa8b3bb), and Mother Brain's following explosions leave their
/// offsets as velocity for a later projectile in the same room (<see cref="FollowingExplosionLeftVelocity"/>).
/// </remarks>
/// <param name="Position">The previous projectile's whole position; its sub-pixel part does not survive.</param>
/// <param name="Velocity">The previous projectile's velocity.</param>
[CompatibilityService("an enemy projectile that does not set its position or velocity starts from the whole position and velocity its place's previous projectile left, in this room or the room before")]
public readonly record struct EnemyProjectileHandOver(Position16<RoomPixels> Position, EightEightVector<RoomPixels> Velocity);

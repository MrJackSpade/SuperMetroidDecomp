namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The pre-Phantoon room's setup holds one enemy-projectile place for its first projectile updates: a blank, harmless
/// projectile takes the first place a spawn would take and deletes itself after <see cref="HeldUpdates"/> updates. While
/// it holds its place, a projectile spawned in the room (a Work Robot's laser in the powered room) takes the place below
/// it, so it runs, and is scanned against Samus, after projectiles spawned once the place is free again; the pool also
/// has one place fewer.
/// </summary>
/// <remarks>
/// Consulted by the room's setup, when it spawns the holding projectile, and by the room's background rule, which lasts
/// as long as the place is held. Evidence: the room's setup (<c>$8F:C8C8</c>) spawns its projectile into the pool its
/// load has just cleared, and the projectile's animation (<c>$86:A3AA</c>) shows a blank frame for 32 updates and deletes it;
/// spawns search the pool from the place it takes downward.
/// </remarks>
[CompatibilityService("the pre-Phantoon room's setup holds an enemy-projectile place for its first 32 projectile updates, so projectiles spawned meanwhile take the place below it")]
public static class PrePhantoonRoomHeldProjectilePlace
{
    /// <summary>Projectile updates the holding projectile shows its blank frame before it deletes itself.</summary>
    public const int HeldUpdates = 32;
}

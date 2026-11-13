namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// An enemy projectile's definition: the frame behaviour it starts with, its collision half-extents, the damage it
/// deals Samus, its behaviour flags, where it is drawn relative to Samus and whether touching Samus starts a touch reaction.
/// </summary>
/// <param name="FrameBehavior">Frame behaviour installed before the first animation tick.</param>
/// <param name="Radius">The collision half-extents in room pixels.</param>
/// <param name="Damage">Samus contact damage, zero or more.</param>
/// <param name="Properties">The Samus-damage, contact-persistence and shot-blocking behaviours.</param>
/// <param name="Depth">Whether the projectile starts drawn in front of or behind Samus.</param>
/// <param name="SamusTouch">What touching Samus does to the projectile's animation.</param>
internal readonly record struct EnemyProjectileDefinition(
    EnemyProjectileFrameBehavior FrameBehavior,
    Extent16<RoomPixels> Radius,
    int Damage,
    EnemyProjectilePropertyFlags Properties,
    EnemyProjectileDepth Depth,
    EnemyProjectileSamusTouch SamusTouch)
{
    /// <summary>Whether the projectile's damage value affects Samus.</summary>
    internal bool CanDamageSamus => !Properties.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus);
    /// <summary>Whether the projectile is retained after touching Samus.</summary>
    internal bool PersistsOnSamusContact => Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact);
    /// <summary>Whether this projectile intercepts Samus's shots.</summary>
    internal bool BlocksSamusProjectiles => Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots);
}

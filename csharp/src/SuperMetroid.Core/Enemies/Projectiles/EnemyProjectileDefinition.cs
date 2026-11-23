namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// An enemy projectile's definition: its collision half-extents, the damage it deals Samus, its behaviour flags, where it
/// is drawn relative to Samus and whether touching Samus starts a touch reaction. Its motion is its behaviour's (#1843).
/// </summary>
/// <param name="Radius">The collision half-extents in room pixels.</param>
/// <param name="Damage">Samus contact damage, zero or more.</param>
/// <param name="Properties">The Samus-damage, contact-persistence and shot-blocking behaviours.</param>
/// <param name="Depth">Whether the projectile starts drawn in front of or behind Samus.</param>
/// <param name="SamusTouch">What touching Samus does to the projectile's animation; nearly every kind keeps it.</param>
internal readonly partial record struct EnemyProjectileDefinition(
    Extent16<RoomPixels> Radius,
    int Damage,
    EnemyProjectilePropertyFlags Properties,
    EnemyProjectileDepth Depth,
    EnemyProjectileSamusTouch SamusTouch = EnemyProjectileSamusTouch.KeepsAnimation)
{
    /// <summary>
    /// A decorative kind's definition (#2557): no size and no damage, keeping its animation when it touches Samus, so only
    /// its behaviours and where it is drawn differ.
    /// </summary>
    /// <param name="properties">The kind's Samus-damage, contact-persistence and shot-blocking behaviours.</param>
    /// <param name="depth">Whether the kind starts drawn in front of or behind Samus.</param>
    internal static EnemyProjectileDefinition Decorative(EnemyProjectilePropertyFlags properties, EnemyProjectileDepth depth) =>
        new(default, 0, properties, depth);

    /// <summary>Whether the projectile's damage value affects Samus.</summary>
    internal bool CanDamageSamus => !Properties.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus);
    /// <summary>Whether the projectile is retained after touching Samus.</summary>
    internal bool PersistsOnSamusContact => Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact);
    /// <summary>Whether this projectile intercepts Samus's shots.</summary>
    internal bool BlocksSamusProjectiles => Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots);
}

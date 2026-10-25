namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// An enemy projectile's definition: the per-frame callback it starts with, its collision half-extents, the damage it
/// deals Samus, its behaviour flags and whether touching Samus starts a touch reaction.
/// </summary>
/// <param name="PreInstruction">Per-frame callback installed before the first frame-list tick.</param>
/// <param name="Radius">The collision half-extents in room pixels.</param>
/// <param name="Damage">Samus contact damage, 0 through 4095.</param>
/// <param name="Properties">The draw priority, Samus-damage, contact-persistence and shot-blocking behaviours.</param>
/// <param name="SamusTouch">What touching Samus does to the projectile's program.</param>
internal readonly record struct EnemyProjectileDefinition(
    EnemyProjectilePreInstruction PreInstruction,
    Extent16<RoomPixels> Radius,
    int Damage,
    EnemyProjectilePropertyFlags Properties,
    EnemyProjectileSamusTouch SamusTouch)
{
    /// <summary>The draw pass: high priority when the definition says so.</summary>
    internal EnemyProjectileDrawPriority DrawPriority =>
        Properties.HasFlag(EnemyProjectilePropertyFlags.HighPriority)
            ? EnemyProjectileDrawPriority.High
            : EnemyProjectileDrawPriority.Low;
    /// <summary>Whether the projectile's damage value affects Samus.</summary>
    internal bool CanDamageSamus => !Properties.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus);
    /// <summary>Whether the projectile is retained after touching Samus.</summary>
    internal bool PersistsOnSamusContact => Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact);
    /// <summary>Whether this projectile intercepts Samus's shots.</summary>
    internal bool BlocksSamusProjectiles => Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots);
}

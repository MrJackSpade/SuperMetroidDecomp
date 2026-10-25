namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The behaviours a projectile's instruction list switches on and off. The projectile interpreter tests each on its
/// own, so they compose: a list's property OR sets every named behaviour, and its AND clears every behaviour its mask
/// leaves out. Where each sits in the cartridge's property word is the import's knowledge.
/// </summary>
[Flags]
internal enum EnemyProjectilePropertyFlags
{
    /// <summary>Draws at high priority.</summary>
    HighPriority = 1,
    /// <summary>Does not damage Samus.</summary>
    HarmlessToSamus = 2,
    /// <summary>Survives contact with Samus.</summary>
    PersistsOnContact = 4,
    /// <summary>Blocks Samus's projectiles.</summary>
    BlocksShots = 8,
}

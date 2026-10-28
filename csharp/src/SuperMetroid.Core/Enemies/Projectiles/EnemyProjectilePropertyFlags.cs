namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The behaviours a projectile's instruction list switches on and off. The projectile interpreter tests each on its
/// own, so they compose: a list's property OR sets every named behaviour, and its AND clears every behaviour its mask
/// leaves out. Where each sits in the cartridge's property word is the import's knowledge.
/// </summary>
[Flags]
internal enum EnemyProjectilePropertyFlags
{
    /// <summary>None of the behaviours.</summary>
    None = 0,
    /// <summary>Does not damage Samus.</summary>
    HarmlessToSamus = 1,
    /// <summary>Survives contact with Samus.</summary>
    PersistsOnContact = 2,
    /// <summary>Blocks Samus's projectiles.</summary>
    BlocksShots = 4,
}

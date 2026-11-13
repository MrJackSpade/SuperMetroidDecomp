namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The behaviours a projectile's program switches on and off. The projectile pass tests each on its
/// own, so they compose: a program's property set turns on every named behaviour, and its property mask clears every
/// behaviour the mask leaves out. How the game stores them is the import's knowledge.
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

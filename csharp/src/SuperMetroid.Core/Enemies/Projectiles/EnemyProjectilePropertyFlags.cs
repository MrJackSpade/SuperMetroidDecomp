namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The behavior bits of a bank-$86 projectile's property word, above its twelve damage bits. The projectile
/// interpreter tests each bit on its own, so they compose: a list's property OR sets every named bit, and its AND
/// clears every bit its mask leaves out.
/// </summary>
[Flags]
internal enum EnemyProjectilePropertyFlags : ushort
{
    /// <summary>No behavior bit.</summary>
    None = 0,
    /// <summary>Bit 12: draws at high priority.</summary>
    HighPriority = 0x1000,
    /// <summary>Bit 13: does not damage Samus.</summary>
    HarmlessToSamus = 0x2000,
    /// <summary>Bit 14: survives contact with Samus.</summary>
    PersistsOnContact = 0x4000,
    /// <summary>Bit 15: blocks Samus's projectiles.</summary>
    BlocksShots = 0x8000,
}

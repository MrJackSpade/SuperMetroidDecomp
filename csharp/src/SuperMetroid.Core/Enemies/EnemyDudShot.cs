namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The dud shot for normal bombs. The extended walker has already marked the physical bomb; no
/// vulnerability, health or boss state follows. Bomb-owned dud presentation remains a shared projectile-system seam, so
/// the enemy-visible contract ends here.
/// </summary>
internal static class EnemyDudShot
{
    /// <summary>The bomb reaction: nothing beyond the walker's mark.</summary>
    internal static readonly Action<EnemyBombContext> Bomb = static _ => { };
}

namespace SuperMetroid.Core.Enemies;

/// <summary>Collision paths a species takes beyond the common radius-based ones; declared once per species.</summary>
[Flags]
internal enum EnemyCapabilities
{
    /// <summary>Only the common paths.</summary>
    None = 0,

    /// <summary>
    /// Samus's contact with the enemy is tested against its frames' hitboxes. This differs deliberately from the shot
    /// set: Phantoon's frames' hitboxes meet Samus, while his own projectile resolver owns how shots meet him.
    /// </summary>
    ExtendedSamusHitboxes = 1,

    /// <summary>
    /// Samus's shots are tested against the enemy's frames' hitboxes. <see cref="RoomEnemySlot.CollidesByFrameHitboxes"/>
    /// alone is not sufficient: enemies that collide by frame hitboxes may still meet Samus by their radius or by
    /// encounter-specific geometry.
    /// </summary>
    ExtendedProjectileHitboxes = 2,
}

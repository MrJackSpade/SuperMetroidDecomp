namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The mutually exclusive draw pass selected by enemy-projectile property bit
/// <c>$1000</c>. Bank <c>$86:8390/$83B2</c> traverses the same physical pool once for
/// each value, on opposite sides of Samus's OAM submission.
/// </summary>
public enum EnemyProjectileDrawPriority : byte
{
    /// <summary>Property bit $1000 is clear; the projectile is submitted in the low-priority pass after Samus.</summary>
    Low = 0,
    /// <summary>Property bit $1000 is set; the projectile is submitted in the high-priority pass before Samus.</summary>
    High = 1,
}

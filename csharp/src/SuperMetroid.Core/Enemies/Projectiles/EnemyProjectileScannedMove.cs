namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>One axis move of an enemy projectile as its block collision tests it (<see cref="EnemyProjectileBlockCollision"/>).</summary>
/// <param name="Projectile">The projectile moving, at its position before the move.</param>
/// <param name="Scan">The blocks its leading edge crosses; its edge is the move's axis.</param>
/// <param name="MovingNegative">Whether the move goes toward decreasing coordinates, left or up.</param>
/// <param name="LeadingEdge">The leading edge's pixel on the move's axis after the move.</param>
internal readonly partial record struct EnemyProjectileScannedMove(
    EnemyProjectile Projectile,
    EnemyProjectileBlockScan Scan,
    bool MovingNegative,
    int LeadingEdge);

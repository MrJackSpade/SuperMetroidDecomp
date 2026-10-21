using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The x, y, pose, timer and direction of one projectile contact phase expected.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Pose">The pose.</param>
/// <param name="Timer">The timer.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct ProjectileContactPhaseExpected(uint X, uint Y, SamusPoseId Pose, ushort Timer, ushort Direction);

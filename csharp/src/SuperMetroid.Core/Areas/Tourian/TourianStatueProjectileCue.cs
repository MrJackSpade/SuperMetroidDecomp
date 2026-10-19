using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A statue projectile program cue.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Pixels">How far a <see cref="TourianStatueProjectileCueKind.MoveDown"/> cue moves the projectile.</param>
/// <param name="Movement">The routine a <see cref="TourianStatueProjectileCueKind.StartMoving"/> cue installs.</param>
internal readonly record struct TourianStatueProjectileCue(
    TourianStatueProjectileCueKind Kind,
    int Pixels = 0,
    EnemyProjectilePreInstruction Movement = default);

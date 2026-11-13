using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A laser or claw animation cue.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Movement">The movement behaviour it installs.</param>
internal readonly record struct SpacePirateProjectileCue(SpacePirateProjectileCueKind Kind, EnemyProjectileFrameBehavior Movement);

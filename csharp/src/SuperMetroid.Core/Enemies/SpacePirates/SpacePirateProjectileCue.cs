using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A laser or claw program cue.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Movement">The movement routine it installs.</param>
internal readonly record struct SpacePirateProjectileCue(SpacePirateProjectileCueKind Kind, EnemyProjectilePreInstruction Movement);

using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One cue of the eye door's projectile programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="PreInstruction">The movement routine the cue installs.</param>
internal readonly record struct EyeDoorProjectileCue(EyeDoorProjectileCueKind Kind, EnemyProjectilePreInstruction PreInstruction = default);

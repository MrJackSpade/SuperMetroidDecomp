using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>One cue of the eye door's projectile programs, with the operands its command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="FrameBehavior">The movement behaviour the cue installs.</param>
internal readonly record struct EyeDoorProjectileCue(EyeDoorProjectileCueKind Kind, EnemyProjectileFrameBehavior FrameBehavior = default);

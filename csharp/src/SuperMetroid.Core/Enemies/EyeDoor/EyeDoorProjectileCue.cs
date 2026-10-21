using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>One cue of the eye door's projectile programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="PreInstruction">The movement routine the cue installs.</param>
internal readonly record struct EyeDoorProjectileCue(EyeDoorProjectileCueKind Kind, EnemyProjectilePreInstruction PreInstruction = default);

using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>One cue of the n00b tube's crack, shards and bubbles animations, with the operands its command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="FrameBehavior">The movement behaviour the cue installs.</param>
internal readonly record struct NoobTubeProjectileCue(NoobTubeProjectileCueKind Kind, EnemyProjectileFrameBehavior FrameBehavior = default);

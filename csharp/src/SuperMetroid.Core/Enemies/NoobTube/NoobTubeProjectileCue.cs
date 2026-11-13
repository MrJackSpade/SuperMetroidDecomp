using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>One cue of the n00b tube's crack, shards and bubbles animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="FrameBehavior">The movement behaviour the cue installs.</param>
internal readonly record struct NoobTubeProjectileCue(NoobTubeProjectileCueKind Kind, EnemyProjectileFrameBehavior FrameBehavior = default);

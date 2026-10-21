using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>One cue of the n00b tube's crack, shards and bubbles programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="PreInstruction">The movement routine the cue installs.</param>
internal readonly record struct NoobTubeProjectileCue(NoobTubeProjectileCueKind Kind, EnemyProjectilePreInstruction PreInstruction = default);

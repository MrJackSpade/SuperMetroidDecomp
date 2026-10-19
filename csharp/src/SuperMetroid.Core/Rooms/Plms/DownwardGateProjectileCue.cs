using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One cue of the downward shot gate's programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="PreInstruction">The movement routine the cue installs.</param>
/// <param name="YVelocity">The vertical velocity a <see cref="DownwardGateProjectileCueKind.SetYVelocity"/> cue installs.</param>
internal readonly record struct DownwardGateProjectileCue(
    DownwardGateProjectileCueKind Kind,
    EnemyProjectilePreInstruction PreInstruction = default,
    SnesSignedEightEight YVelocity = default);

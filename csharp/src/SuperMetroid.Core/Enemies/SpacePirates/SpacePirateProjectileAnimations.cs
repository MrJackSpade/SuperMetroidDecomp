using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.SpacePirates.SpacePirateProjectileSegment;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser and Ninja Pirate claw animations (#641).</summary>
internal static class SpacePirateProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SpacePirateProjectileSegment, EnemyProjectileFrameId, SpacePirateProjectileCue> Segments { get; } = new(
        new Dictionary<SpacePirateProjectileSegment, AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>[]>
    {
        [P.LaserLeft] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlint, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlow, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareBall, 2),
            Cue(SpacePirateProjectileCueKind.FireLaser, HorizontalDirection.Left),
            Show(EnemyProjectileFrameId.SpacePirateLaserEmergingBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserEmergingDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserExtendingBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserExtendingDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
        ],
        [P.LaserLeftFlicker] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
        ],
        [P.LaserRight] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlint, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlow, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareBall, 2),
            Cue(SpacePirateProjectileCueKind.FireLaser, HorizontalDirection.Right),
            Show(EnemyProjectileFrameId.SpacePirateLaserEmergingBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserEmergingDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserExtendingBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserExtendingDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
        ],
        [P.LaserRightFlicker] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
        ],
        [P.ClawLeft] =
        [
            Cue(SpacePirateProjectileCueKind.ThrowClaw, HorizontalDirection.Left),
        ],
        [P.ClawLeftSpin] =
        [
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned0, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned45, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned90, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned135, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned180, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned225, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned270, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned315, 1),
        ],
        [P.ClawRight] =
        [
            Cue(SpacePirateProjectileCueKind.ThrowClaw, HorizontalDirection.Right),
        ],
        [P.ClawRightSpin] =
        [
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned0, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned45, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned90, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned135, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned180, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned225, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned270, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned315, 1),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each segment played whole: a laser's
    /// muzzle and flight, then its flicker looping; a claw's throw, then its spin looping (#2276).
    /// </summary>
    internal static IReadOnlyDictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>> StartingAt { get; } = new Dictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>>
    {
        [P.LaserLeft] = new([Segments.Whole(P.LaserLeft), Segments.Whole(P.LaserLeftFlicker)], AnimationEnd.LoopsFrom(1)),
        [P.LaserRight] = new([Segments.Whole(P.LaserRight), Segments.Whole(P.LaserRightFlicker)], AnimationEnd.LoopsFrom(1)),
        [P.ClawLeft] = new([Segments.Whole(P.ClawLeft), Segments.Whole(P.ClawLeftSpin)], AnimationEnd.LoopsFrom(1)),
        [P.ClawRight] = new([Segments.Whole(P.ClawRight), Segments.Whole(P.ClawRightSpin)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SpacePirateProjectileSegment> StartingWith(SpacePirateProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes a cue of <paramref name="kind"/> toward <paramref name="direction"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue> Cue(
        SpacePirateProjectileCueKind kind, HorizontalDirection direction) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Event(new(kind, direction));
}

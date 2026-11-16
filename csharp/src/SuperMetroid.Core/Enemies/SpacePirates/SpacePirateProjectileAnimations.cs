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
            Show(EnemyProjectileFrameId.SpacePirateLaserBright8, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim8, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserBright16, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim16, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserBright24, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim24, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserBright32, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim32, 1),
        ],
        [P.LaserLeftLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserBright32Twin, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim32Twin, 1),
        ],
        [P.LaserRight] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlintTwin, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlowTwin, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareBallTwin, 2),
            Cue(SpacePirateProjectileCueKind.FireLaser, HorizontalDirection.Right),
            Show(EnemyProjectileFrameId.SpacePirateLaserBright8Twin, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim8Twin, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserBright16Twin, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim16Twin, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserBright24Twin, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim24Twin, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserBright32TwinB, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim32TwinB, 1),
        ],
        [P.LaserRightLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserBright32TwinC, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserDim32TwinC, 1),
        ],
        [P.ClawLeft] =
        [
            Cue(SpacePirateProjectileCueKind.ThrowClaw, HorizontalDirection.Left),
        ],
        [P.ClawLeftLoop] =
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
        [P.ClawRightLoop] =
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

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>> StartingAt { get; } = new Dictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>>
    {
        [P.LaserLeft] = new([new(P.LaserLeft, 0, 12, 1), new(P.LaserLeftLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LaserLeftLoop] = new([new(P.LaserLeftLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LaserRight] = new([new(P.LaserRight, 0, 12, 1), new(P.LaserRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LaserRightLoop] = new([new(P.LaserRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClawLeft] = new([new(P.ClawLeft, 0, 1, 1), new(P.ClawLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ClawLeftLoop] = new([new(P.ClawLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClawRight] = new([new(P.ClawRight, 0, 1, 1), new(P.ClawRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ClawRightLoop] = new([new(P.ClawRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
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

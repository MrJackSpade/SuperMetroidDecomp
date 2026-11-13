using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.SpacePirates.SpacePirateProjectileProgram;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser and Ninja Pirate claw programs (#641).</summary>
internal static class SpacePirateProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SpacePirateProjectileProgram, EnemyProjectileFrameId, SpacePirateProjectileCue> Segments { get; } = new(
        new Dictionary<SpacePirateProjectileProgram, AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>[]>
    {
        [P.LaserLeft] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftFirst, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftSecond, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftThird, 2),
            Cue(SpacePirateProjectileCueKind.StartMovingNow, EnemyProjectileFrameBehavior.LaserFlightLeft),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftFourth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftFifth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftSixth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftSeventh, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftEighth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftNinth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftTenth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftEleventh, 1),
        ],
        [P.LaserLeftLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftLoopFirst, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLeftLoopSecond, 1),
        ],
        [P.LaserRight] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserRightFirst, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightSecond, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightThird, 2),
            Cue(SpacePirateProjectileCueKind.StartMovingNow, EnemyProjectileFrameBehavior.LaserFlightRight),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightFourth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightFifth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightSixth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightSeventh, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightEighth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightNinth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightTenth, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightEleventh, 1),
        ],
        [P.LaserRightLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserRightLoopFirst, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserRightLoopSecond, 1),
        ],
        [P.ClawLeft] =
        [
            Cue(SpacePirateProjectileCueKind.StartMoving, EnemyProjectileFrameBehavior.PirateClawThrownLeft),
        ],
        [P.ClawLeftLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateClawLeftLoopFirst, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftLoopSecond, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftLoopThird, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftLoopFourth, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftLoopFifth, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftLoopSixth, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftLoopSeventh, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftLoopEighth, 1),
        ],
        [P.ClawRight] =
        [
            Cue(SpacePirateProjectileCueKind.StartMoving, EnemyProjectileFrameBehavior.PirateClawThrownRight),
        ],
        [P.ClawRightLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateClawRightLoopFirst, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightLoopSecond, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightLoopThird, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightLoopFourth, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightLoopFifth, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightLoopSixth, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightLoopSeventh, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightLoopEighth, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SpacePirateProjectileProgram, Animation<SpacePirateProjectileProgram>> StartingAt { get; } = new Dictionary<SpacePirateProjectileProgram, Animation<SpacePirateProjectileProgram>>
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
    internal static Animation<SpacePirateProjectileProgram> StartingWith(SpacePirateProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes a cue of <paramref name="kind"/> installing <paramref name="movement"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue> Cue(
        SpacePirateProjectileCueKind kind, EnemyProjectileFrameBehavior movement) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Event(new(kind, movement));
}

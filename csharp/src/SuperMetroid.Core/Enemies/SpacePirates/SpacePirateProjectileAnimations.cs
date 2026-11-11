using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.SpacePirates.SpacePirateProjectileProgram;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser and Ninja Pirate claw programs (#641).</summary>
internal static class SpacePirateProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue> Segments { get; } = new(
        new Dictionary<SpacePirateProjectileProgram, AnimationBeat<SpacePirateProjectileFrame, SpacePirateProjectileCue>[]>
    {
        [P.LaserLeft] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Cue(SpacePirateProjectileCueKind.StartMovingNow, EnemyProjectileFrameBehavior.LaserFlightLeft),
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
        ],
        [P.LaserLeftLoop] =
        [
            Show(11, 1),
            Show(12, 1),
        ],
        [P.LaserRight] =
        [
            Show(13, 2),
            Show(14, 2),
            Show(15, 2),
            Cue(SpacePirateProjectileCueKind.StartMovingNow, EnemyProjectileFrameBehavior.LaserFlightRight),
            Show(16, 1),
            Show(17, 1),
            Show(18, 1),
            Show(19, 1),
            Show(20, 1),
            Show(21, 1),
            Show(22, 1),
            Show(23, 1),
        ],
        [P.LaserRightLoop] =
        [
            Show(24, 1),
            Show(25, 1),
        ],
        [P.ClawLeft] =
        [
            Cue(SpacePirateProjectileCueKind.StartMoving, EnemyProjectileFrameBehavior.PirateClawThrownLeft),
        ],
        [P.ClawLeftLoop] =
        [
            Show(26, 1),
            Show(27, 1),
            Show(28, 1),
            Show(29, 1),
            Show(30, 1),
            Show(31, 1),
            Show(32, 1),
            Show(33, 1),
        ],
        [P.ClawRight] =
        [
            Cue(SpacePirateProjectileCueKind.StartMoving, EnemyProjectileFrameBehavior.PirateClawThrownRight),
        ],
        [P.ClawRightLoop] =
        [
            Show(34, 1),
            Show(35, 1),
            Show(36, 1),
            Show(37, 1),
            Show(38, 1),
            Show(39, 1),
            Show(40, 1),
            Show(41, 1),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SpacePirateProjectileFrame, SpacePirateProjectileCue> Show(int pose, int duration) =>
        new AnimationBeat<SpacePirateProjectileFrame, SpacePirateProjectileCue>.Frame(new(pose), duration);

    /// <summary>Publishes a cue of <paramref name="kind"/> installing <paramref name="movement"/>.</summary>
    private static AnimationBeat<SpacePirateProjectileFrame, SpacePirateProjectileCue> Cue(
        SpacePirateProjectileCueKind kind, EnemyProjectileFrameBehavior movement) =>
        new AnimationBeat<SpacePirateProjectileFrame, SpacePirateProjectileCue>.Event(new(kind, movement));
}

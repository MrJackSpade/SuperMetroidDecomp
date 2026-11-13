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
            Show(EnemyProjectileFrameId.SpacePirateFrame00, 2),
            Show(EnemyProjectileFrameId.SpacePirateFrame01, 2),
            Show(EnemyProjectileFrameId.SpacePirateFrame02, 2),
            Cue(SpacePirateProjectileCueKind.StartMovingNow, EnemyProjectileFrameBehavior.LaserFlightLeft),
            Show(EnemyProjectileFrameId.SpacePirateFrame03, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame04, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame05, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame06, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame07, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame08, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame09, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame10, 1),
        ],
        [P.LaserLeftLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateFrame11, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame12, 1),
        ],
        [P.LaserRight] =
        [
            Show(EnemyProjectileFrameId.SpacePirateFrame13, 2),
            Show(EnemyProjectileFrameId.SpacePirateFrame14, 2),
            Show(EnemyProjectileFrameId.SpacePirateFrame15, 2),
            Cue(SpacePirateProjectileCueKind.StartMovingNow, EnemyProjectileFrameBehavior.LaserFlightRight),
            Show(EnemyProjectileFrameId.SpacePirateFrame16, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame17, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame18, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame19, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame20, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame21, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame22, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame23, 1),
        ],
        [P.LaserRightLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateFrame24, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame25, 1),
        ],
        [P.ClawLeft] =
        [
            Cue(SpacePirateProjectileCueKind.StartMoving, EnemyProjectileFrameBehavior.PirateClawThrownLeft),
        ],
        [P.ClawLeftLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateFrame26, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame27, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame28, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame29, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame30, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame31, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame32, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame33, 1),
        ],
        [P.ClawRight] =
        [
            Cue(SpacePirateProjectileCueKind.StartMoving, EnemyProjectileFrameBehavior.PirateClawThrownRight),
        ],
        [P.ClawRightLoop] =
        [
            Show(EnemyProjectileFrameId.SpacePirateFrame34, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame35, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame36, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame37, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame38, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame39, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame40, 1),
            Show(EnemyProjectileFrameId.SpacePirateFrame41, 1),
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

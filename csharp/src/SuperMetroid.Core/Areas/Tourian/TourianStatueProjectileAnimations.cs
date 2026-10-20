using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using K = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileCueKind;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileProgram;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian statue projectile programs (#641), in place of their bank-$86 lists.</summary>
internal static class TourianStatueProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> Programs { get; } = new(
        new Dictionary<TourianStatueProjectileProgram, AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>[]>
    {
        [P.Delete] =
        [
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.Splash] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.EyeGlow] =
        [
            Show(4, 8),
            Show(5, 8),
            Show(6, 8),
            Show(7, 7),
            Show(8, 7),
            Show(9, 7),
            Show(10, 6),
            Show(11, 6),
            Show(12, 5),
            Show(13, 48),
            Cue(new TourianStatueProjectileCue.Command(K.PlayReleaseSound)),
            Cue(new TourianStatueProjectileCue.Command(K.Earthquake)),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.Particle] =
        [
            Show(14, 3),
            Show(15, 3),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnTail)),
            Show(16, 3),
            Show(17, 3),
            RepeatWhileCounting(P.Particle),
            Hold,
        ],
        [P.Tail] =
        [
            Show(18, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 8)),
            Show(19, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 4)),
            Show(20, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 2)),
            Show(21, 4),
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.Soul] =
        [
            Show(22, 8),
            Show(23, 8),
            GoTo(P.Soul),
        ],
        [P.BaseDecoration] =
        [
            Show(24, 128),
            Cue(new TourianStatueProjectileCue.StartMoving(Movement: EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess)),
            FallInto(P.BaseDecorationLoop),
        ],
        [P.BaseDecorationLoop] =
        [
            Show(25, 1911),
            GoTo(P.BaseDecorationLoop),
        ],
        [P.Ridley] =
        [
            Show(26, 1911),
            GoTo(P.Ridley),
        ],
        [P.Phantoon] =
        [
            Show(27, 1911),
            GoTo(P.Phantoon),
        ],
    });

    /// <summary>Holds the current frame for good.</summary>
    private static AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> Hold => AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>.Hold;

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> Show(int pose, int duration) => AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> Cue(TourianStatueProjectileCue cue) => AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>.Publish(cue);

    /// <summary>Repeats <paramref name="program"/> while the projectile's counter counts down.</summary>
    private static AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> RepeatWhileCounting(TourianStatueProjectileProgram program) => AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>.RepeatWhileCounting(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> FallInto(TourianStatueProjectileProgram program) => AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>.FallInto(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> GoTo(TourianStatueProjectileProgram program) => AnimationProgramStep<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>.GoTo(program);
}

using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoTourianDustProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo's footstep and explosion dust and the Tourian statue's descent dust (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="ChozoTourianDustFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class ChozoTourianDustAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<ChozoTourianDustProgram, AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Footsteps] =
        [
            Cue(new(CK.Scatter, Scatter: new(Range: new(15, 15), Center: new(0, 3)))),
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Cue(new(CK.Delete)),
            Hold,
        ],
        [P.SpikeClearingExplosions] =
        [
            Cue(new(CK.Scatter, Scatter: new(Range: new(15, 15), Center: new(0, 3)))),
            Show(4, 5),
            Show(5, 5),
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            Cue(new(CK.Delete)),
            Hold,
        ],
        [P.TourianDescentDust] =
        [
            SetCounter(64),
            FallInto(P.TourianDescentDustLoop),
        ],
        [P.TourianDescentDustLoop] =
        [
            Cue(new(CK.ReturnToOrigin)),
            Cue(new(CK.Scatter, Scatter: new(Range: new(63, 3), Center: new(0, 0)))),
            Show(10, 2),
            Show(11, 2),
            Show(12, 2),
            Show(13, 2),
            RepeatWhileCounting(P.TourianDescentDustLoop),
            Cue(new(CK.Delete)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> SetCounter(int count) => AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> RepeatWhileCounting(ChozoTourianDustProgram program) => AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>.RepeatWhileCounting(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> FallInto(ChozoTourianDustProgram program) => AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>.Hold;
}

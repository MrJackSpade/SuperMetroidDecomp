using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Evir.EvirCueKind;
using P = SuperMetroid.Core.Enemies.Evir.EvirProgram;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="EvirFrame"/> pose and each callback becomes a <see cref="EvirCue"/> with its typed operands.
/// </summary>
internal static class EvirAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<EvirProgram, EvirFrame, EvirCue> Programs { get; } = new(
        new Dictionary<EvirProgram, AnimationProgramStep<EvirProgram, EvirFrame, EvirCue>[]>
    {
        [P.BodyFacingLeft] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            GoTo(P.BodyFacingLeft),
        ],
        [P.ArmsFacingLeft] =
        [
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 48),
            GoTo(P.ArmsFacingLeft),
        ],
        [P.BodyFacingRight] =
        [
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
            Show(16, 10),
            Show(17, 10),
            Show(18, 10),
            GoTo(P.BodyFacingRight),
        ],
        [P.ArmsFacingRight] =
        [
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(23, 48),
            GoTo(P.ArmsFacingRight),
        ],
        [P.ProjectileNormal] =
        [
            Show(12, 1),
            Hold,
        ],
        [P.ProjectileRegenerating] =
        [
            Cue(new(K.SetInitialRegenerationOffset)),
            SetCounter(8),
            Cue(new(K.PlaySpitSound)),
            FallInto(P.ProjectileRegenerationLoop),
        ],
        [P.ProjectileRegenerationLoop] =
        [
            Show(11, 8),
            Cue(new(K.AdvanceRegenerationOffset)),
            RepeatWhileCounting(P.ProjectileRegenerationLoop),
            Show(12, 16),
            Cue(new(K.FinishRegeneration)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<EvirProgram, EvirFrame, EvirCue> Show(int pose, int duration) => AnimationProgramStep<EvirProgram, EvirFrame, EvirCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<EvirProgram, EvirFrame, EvirCue> Cue(EvirCue cue) => AnimationProgramStep<EvirProgram, EvirFrame, EvirCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<EvirProgram, EvirFrame, EvirCue> SetCounter(int count) => AnimationProgramStep<EvirProgram, EvirFrame, EvirCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<EvirProgram, EvirFrame, EvirCue> RepeatWhileCounting(EvirProgram program) => AnimationProgramStep<EvirProgram, EvirFrame, EvirCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<EvirProgram, EvirFrame, EvirCue> GoTo(EvirProgram program) => AnimationProgramStep<EvirProgram, EvirFrame, EvirCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<EvirProgram, EvirFrame, EvirCue> FallInto(EvirProgram program) => AnimationProgramStep<EvirProgram, EvirFrame, EvirCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<EvirProgram, EvirFrame, EvirCue> Hold => AnimationProgramStep<EvirProgram, EvirFrame, EvirCue>.Hold;

}

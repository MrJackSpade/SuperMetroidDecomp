using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.YappingMaws.YappingMawCueKind;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawProgram;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's eight attack bites and six cooldown animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="YappingMawFrame"/> pose and each callback becomes a <see cref="YappingMawCue"/> with its typed operands.
/// </summary>
internal static class YappingMawAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<YappingMawProgram, YappingMawFrame, YappingMawCue> Programs { get; } = new(
        new Dictionary<YappingMawProgram, AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue>[]>
    {
        [P.AttackingUp] =
        [
            Show(0, 5),
            Show(1, 3),
            Cue(new(K.PlayBiteSound)),
            Show(2, 80),
            Show(1, 3),
            GoTo(P.AttackingUp),
        ],
        [P.AttackingUpRight] =
        [
            Show(3, 5),
            Show(4, 3),
            Cue(new(K.PlayBiteSound)),
            Show(5, 80),
            Show(4, 3),
            GoTo(P.AttackingUpRight),
        ],
        [P.AttackingRight] =
        [
            Show(6, 5),
            Show(7, 3),
            Cue(new(K.PlayBiteSound)),
            Show(8, 80),
            Show(7, 3),
            GoTo(P.AttackingRight),
        ],
        [P.AttackingDownRight] =
        [
            Show(9, 5),
            Show(10, 3),
            Cue(new(K.PlayBiteSound)),
            Show(11, 80),
            Show(10, 3),
            GoTo(P.AttackingDownRight),
        ],
        [P.AttackingDown] =
        [
            Show(12, 5),
            Show(13, 3),
            Cue(new(K.PlayBiteSound)),
            Show(14, 80),
            Show(13, 3),
            GoTo(P.AttackingDown),
        ],
        [P.AttackingDownLeft] =
        [
            Show(15, 5),
            Show(16, 3),
            Cue(new(K.PlayBiteSound)),
            Show(17, 80),
            Show(16, 3),
            GoTo(P.AttackingDownLeft),
        ],
        [P.AttackingLeft] =
        [
            Show(18, 5),
            Show(19, 3),
            Cue(new(K.PlayBiteSound)),
            Show(20, 80),
            Show(19, 3),
            GoTo(P.AttackingLeft),
        ],
        [P.AttackingUpLeft] =
        [
            Show(21, 5),
            Show(22, 3),
            Cue(new(K.PlayBiteSound)),
            Show(23, 80),
            Show(22, 3),
            GoTo(P.AttackingUpLeft),
        ],
        [P.CooldownUpRight] =
        [
            Cue(new(K.HoldSamusUpRight)),
            Show(5, 4),
            FallInto(P.CooldownUp),
        ],
        [P.CooldownUp] =
        [
            Cue(new(K.HoldSamusUp)),
            FallInto(P.CooldownUpLoop),
        ],
        [P.CooldownUpLoop] =
        [
            Show(2, 80),
            Show(1, 3),
            Show(0, 5),
            Show(1, 3),
            Cue(new(K.PlayBiteSound)),
            GoTo(P.CooldownUpLoop),
        ],
        [P.CooldownUpLeft] =
        [
            Cue(new(K.HoldSamusUpLeft)),
            Show(23, 4),
            Cue(new(K.HoldSamusUp)),
            FallInto(P.CooldownUpLeftLoop),
        ],
        [P.CooldownUpLeftLoop] =
        [
            Show(2, 80),
            Show(1, 3),
            Show(0, 5),
            Show(1, 3),
            Cue(new(K.PlayBiteSound)),
            GoTo(P.CooldownUpLeftLoop),
        ],
        [P.CooldownDownRight] =
        [
            Cue(new(K.HoldSamusDownRight)),
            Show(11, 4),
            FallInto(P.CooldownDown),
        ],
        [P.CooldownDown] =
        [
            Cue(new(K.HoldSamusDown)),
            FallInto(P.CooldownDownLoop),
        ],
        [P.CooldownDownLoop] =
        [
            Show(14, 80),
            Show(13, 3),
            Show(12, 5),
            Show(13, 3),
            Cue(new(K.PlayBiteSound)),
            GoTo(P.CooldownDownLoop),
        ],
        [P.CooldownDownLeft] =
        [
            Cue(new(K.HoldSamusDownLeft)),
            Show(17, 4),
            Cue(new(K.HoldSamusDown)),
            FallInto(P.CooldownDownLeftLoop),
        ],
        [P.CooldownDownLeftLoop] =
        [
            Show(14, 80),
            Show(13, 3),
            Show(12, 5),
            Show(13, 3),
            Cue(new(K.PlayBiteSound)),
            GoTo(P.CooldownDownLeftLoop),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params YappingMawProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .OfType<AnimationProgramStep<P, YappingMawFrame, YappingMawCue>.ShowStep>()
            .Select(step => YappingMawFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue> Show(int pose, int duration) => AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue> Cue(YappingMawCue cue) => AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue> GoTo(YappingMawProgram program) => AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue> FallInto(YappingMawProgram program) => AnimationProgramStep<YappingMawProgram, YappingMawFrame, YappingMawCue>.FallInto(program);

}

using K = SuperMetroid.Core.Enemies.Skree.SkreeMetareeCueKind;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeProgram;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="SkreeMetareeFrame"/> pose and each callback becomes a <see cref="SkreeMetareeCue"/> with its typed operands.
/// </summary>
internal static class SkreeMetareeAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue> Programs { get; } = new(
        new Dictionary<SkreeMetareeProgram, AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue>[]>
    {
        // $A3:8910
        [P.MetareeIdling] =
        [
            Show(0, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            GoTo(P.MetareeIdling),
        ],
        // $A3:8924
        [P.MetareePreparingAttack] =
        [
            Show(0, 16),
            Show(1, 8),
            Cue(new(K.SetAttackReady)),
            Hold,
        ],
        // $A3:8930
        [P.MetareeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
            FallInto(P.MetareeDivingLoop),
        ],
        // $A3:8932
        [P.MetareeDivingLoop] =
        [
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(0, 2),
            GoTo(P.MetareeDivingLoop),
        ],
        // $A3:8946
        [P.MetareeStopAnimating] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(0, 1),
            Hold,
        ],
        // $A3:C65E
        [P.SkreeIdling] =
        [
            Show(5, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            GoTo(P.SkreeIdling),
        ],
        // $A3:C672
        [P.SkreePreparingAttack] =
        [
            Show(5, 16),
            Show(6, 8),
            Cue(new(K.SetAttackReady)),
            Hold,
        ],
        // $A3:C67E
        [P.SkreeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
            FallInto(P.SkreeDivingLoop),
        ],
        // $A3:C680
        [P.SkreeDivingLoop] =
        [
            Show(7, 2),
            Show(8, 2),
            Show(9, 2),
            Show(5, 2),
            GoTo(P.SkreeDivingLoop),
        ],
        // $A3:C694
        [P.SkreeStopAnimating] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(5, 1),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SkreeMetareeProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => SkreeMetareeFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue> Show(int pose, int duration) => AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue> Cue(SkreeMetareeCue cue) => AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue> GoTo(SkreeMetareeProgram program) => AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue> FallInto(SkreeMetareeProgram program) => AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue> Hold => AnimationProgramStep<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue>.Hold;

}

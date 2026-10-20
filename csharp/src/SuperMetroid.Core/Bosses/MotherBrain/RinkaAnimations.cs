using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.MotherBrain.RinkaCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.RinkaProgram;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' hide, fire and spin animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="RinkaFrame"/> pose and each callback becomes a <see cref="RinkaCue"/> with its typed operands.
/// </summary>
internal static class RinkaAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<RinkaProgram, RinkaFrame, RinkaCue> Programs { get; } = new(
        new Dictionary<RinkaProgram, AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue>[]>
    {
        [P.Ordinary] =
        [
            Cue(new(K.Hide)),
            Show(0, 64),
            Cue(new(K.Fire)),
            FallInto(P.OrdinaryLoop),
        ],
        [P.OrdinaryLoop] =
        [
            Show(0, 16),
            Show(1, 8),
            Show(2, 7),
            Show(3, 6),
            Show(4, 5),
            Show(3, 6),
            Show(2, 7),
            Show(1, 8),
            GoTo(P.OrdinaryLoop),
        ],
        [P.Special] =
        [
            Cue(new(K.HideAndProcessOffScreen)),
            Show(0, 64),
            Cue(new(K.Fire)),
            FallInto(P.SpecialLoop),
        ],
        [P.SpecialLoop] =
        [
            Show(0, 16),
            Show(1, 8),
            Show(2, 7),
            Show(3, 6),
            Show(4, 5),
            Show(3, 6),
            Show(2, 7),
            Show(1, 8),
            GoTo(P.SpecialLoop),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RinkaProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => RinkaFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue> Show(int pose, int duration) => AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue> Cue(RinkaCue cue) => AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue> GoTo(RinkaProgram program) => AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue> FallInto(RinkaProgram program) => AnimationProgramStep<RinkaProgram, RinkaFrame, RinkaCue>.FallInto(program);

}

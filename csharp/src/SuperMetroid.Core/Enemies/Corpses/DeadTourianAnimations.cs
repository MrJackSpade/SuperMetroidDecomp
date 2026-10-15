using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Corpses.DeadTourianCueKind;
using P = SuperMetroid.Core.Enemies.Corpses.DeadTourianProgram;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed programs (#641), in place of their bank-$A9 lists. Generated from the verified native lists: each
/// frame shows a <see cref="DeadTourianFrame"/> pose and each callback becomes a <see cref="DeadTourianCue"/> with its typed operands.
/// </summary>
internal static class DeadTourianAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<DeadTourianProgram, DeadTourianFrame, DeadTourianCue> Programs { get; } = new(
        new Dictionary<DeadTourianProgram, AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue>[]>
    {
        // $A9:ECAC
        [P.SidehopperHopping] =
        [
            Show(10, 2),
            Show(11, 4),
            Show(10, 5),
            Show(11, 48),
            Show(10, 5),
            Show(11, 4),
            Show(10, 5),
            Show(11, 4),
            Cue(new(K.EndHop)),
            Hold,
        ],
        // $A9:ECE3
        [P.SidehopperIdle] =
        [
            Show(12, 1),
            Hold,
        ],
        // $A9:ECE9
        [P.SidehopperCorpse] =
        [
            Show(0, 1),
            Hold,
        ],
        // $A9:ECEF
        [P.SidehopperInitiallyDead] =
        [
            Show(1, 1),
            Hold,
        ],
        // $A9:ECF5
        [P.ZoomerCorpse0] =
        [
            Show(2, 1),
            Hold,
        ],
        // $A9:ECFB
        [P.ZoomerCorpse1] =
        [
            Show(3, 1),
            Hold,
        ],
        // $A9:ED01
        [P.ZoomerCorpse2] =
        [
            Show(4, 1),
            Hold,
        ],
        // $A9:ED07
        [P.RipperCorpse0] =
        [
            Show(5, 1),
            Hold,
        ],
        // $A9:ED0D
        [P.RipperCorpse1] =
        [
            Show(6, 1),
            Hold,
        ],
        // $A9:ED13
        [P.SkreeCorpse0] =
        [
            Show(7, 1),
            Hold,
        ],
        // $A9:ED19
        [P.SkreeCorpse1] =
        [
            Show(8, 1),
            Hold,
        ],
        // $A9:ED1F
        [P.SkreeCorpse2] =
        [
            Show(9, 1),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DeadTourianProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => DeadTourianFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue> Show(int pose, int duration) => AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue> Cue(DeadTourianCue cue) => AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue> Hold => AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue>.Hold;

}

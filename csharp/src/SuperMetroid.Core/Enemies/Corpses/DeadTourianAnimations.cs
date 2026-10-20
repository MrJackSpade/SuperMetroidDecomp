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
        [P.SidehopperIdle] =
        [
            Show(12, 1),
            Hold,
        ],
        [P.SidehopperCorpse] =
        [
            Show(0, 1),
            Hold,
        ],
        [P.SidehopperInitiallyDead] =
        [
            Show(1, 1),
            Hold,
        ],
        [P.ZoomerCorpse0] =
        [
            Show(2, 1),
            Hold,
        ],
        [P.ZoomerCorpse1] =
        [
            Show(3, 1),
            Hold,
        ],
        [P.ZoomerCorpse2] =
        [
            Show(4, 1),
            Hold,
        ],
        [P.RipperCorpse0] =
        [
            Show(5, 1),
            Hold,
        ],
        [P.RipperCorpse1] =
        [
            Show(6, 1),
            Hold,
        ],
        [P.SkreeCorpse0] =
        [
            Show(7, 1),
            Hold,
        ],
        [P.SkreeCorpse1] =
        [
            Show(8, 1),
            Hold,
        ],
        [P.SkreeCorpse2] =
        [
            Show(9, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue> Show(int pose, int duration) => AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue> Cue(DeadTourianCue cue) => AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue> Hold => AnimationProgramStep<DeadTourianProgram, DeadTourianFrame, DeadTourianCue>.Hold;

}

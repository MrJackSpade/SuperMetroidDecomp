using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.NorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.NorfairRioProgram;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Norfair Rio's idle and swoop animations and its flames' animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="NorfairRioFrame"/> pose and each callback becomes a <see cref="NorfairRioCue"/> with its typed operands.
/// </summary>
internal static class NorfairRioAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<NorfairRioProgram, NorfairRioFrame, NorfairRioCue> Programs { get; } = new(
        new Dictionary<NorfairRioProgram, AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.FlamesAt8)),
            Show(1, 13),
            Show(0, 18),
            Show(1, 13),
            Show(2, 18),
            GoTo(P.Idle),
        ],
        [P.StartDescending] =
        [
            Cue(new(K.FlamesAt8)),
            Show(0, 1),
            Cue(new(K.FlamesAt4)),
            Show(9, 1),
            Cue(new(K.FlamesAt0)),
            Show(10, 1),
            Cue(new(K.FlamesAtMinus4)),
            Show(11, 1),
            Cue(new(K.FlamesAtMinus12)),
            Show(12, 1),
            Cue(new(K.FlamesAtMinus16)),
            Show(13, 1),
            Cue(new(K.SwoopStartFinished)),
            Hold,
        ],
        [P.Descending] =
        [
            Cue(new(K.FlamesAtMinus12)),
            Show(6, 6),
            Show(7, 5),
            Show(8, 8),
            Show(7, 6),
            GoTo(P.Descending),
        ],
        [P.StartAscending] =
        [
            Cue(new(K.FlamesAtMinus16)),
            Show(13, 1),
            Cue(new(K.FlamesAtMinus12)),
            Show(12, 1),
            Cue(new(K.FlamesAtMinus4)),
            Show(11, 1),
            Cue(new(K.FlamesAt0)),
            Show(10, 1),
            Cue(new(K.FlamesAt4)),
            Show(9, 1),
            Cue(new(K.FlamesAt8)),
            Show(0, 1),
            Cue(new(K.FlamesAt8)),
            Show(1, 1),
            Cue(new(K.FlamesAt12)),
            Show(2, 1),
            Cue(new(K.SwoopStartFinished)),
            Hold,
        ],
        [P.Ascending] =
        [
            Cue(new(K.FlamesAt12)),
            Show(14, 6),
            Show(15, 5),
            Show(16, 8),
            Show(15, 6),
            GoTo(P.Ascending),
        ],
        [P.FlamesAscending] =
        [
            Show(3, 6),
            Show(4, 5),
            Show(5, 8),
            Show(4, 6),
            GoTo(P.FlamesAscending),
        ],
        [P.FlamesDescending] =
        [
            Show(17, 6),
            Show(18, 5),
            Show(19, 8),
            Show(18, 6),
            GoTo(P.FlamesDescending),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue> Show(int pose, int duration) => AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue> Cue(NorfairRioCue cue) => AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue> GoTo(NorfairRioProgram program) => AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue> Hold => AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue>.Hold;

}

using P = SuperMetroid.Core.Enemies.Kago.KagoProgram;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Kago's slow and post-hit animation loops as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="KagoFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class KagoAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<KagoProgram, KagoFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<KagoProgram, AnimationProgramStep<KagoProgram, KagoFrame, NoAnimationCue>[]>
    {
        // $A8:AB1E
        [P.Slow] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(1, 10),
            GoTo(P.Slow),
        ],
        // $A8:AB32
        [P.Fast] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(1, 3),
            GoTo(P.Fast),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<KagoProgram, KagoFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<KagoProgram, KagoFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<KagoProgram, KagoFrame, NoAnimationCue> GoTo(KagoProgram program) => AnimationProgramStep<KagoProgram, KagoFrame, NoAnimationCue>.GoTo(program);

}

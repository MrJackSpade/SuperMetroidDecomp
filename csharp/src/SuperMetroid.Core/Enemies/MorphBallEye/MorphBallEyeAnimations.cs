using P = SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeProgram;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The Morph Ball eye's tracking, eyelid and mount animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MorphBallEyeFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class MorphBallEyeAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<MorphBallEyeProgram, AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue>[]>
    {
        [P.Active] =
        [
            Show(7, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            Show(11, 10),
            Show(12, 10),
            Show(13, 10),
            Show(6, 10),
            Show(5, 10),
            Show(4, 10),
            Show(3, 10),
            Show(2, 10),
            Show(1, 10),
            Show(0, 10),
            Show(0, 10),
            GoTo(P.Active),
        ],
        [P.FacingRightDeactivating] =
        [
            Show(15, 8),
            Show(3, 48),
            Show(15, 5),
            FallInto(P.FacingRightClosed),
        ],
        [P.FacingRightClosed] =
        [
            Show(14, 48),
            Hold,
        ],
        [P.FacingLeftDeactivating] =
        [
            Show(17, 8),
            Show(10, 48),
            Show(17, 5),
            FallInto(P.FacingLeftClosed),
        ],
        [P.FacingLeftClosed] =
        [
            Show(16, 48),
            Hold,
        ],
        [P.FacingRightActivating] =
        [
            Show(14, 32),
            Show(15, 5),
            Show(3, 48),
            Show(15, 8),
            Hold,
        ],
        [P.FacingLeftActivating] =
        [
            Show(16, 32),
            Show(17, 5),
            Show(10, 48),
            Show(17, 8),
            Hold,
        ],
        [P.MountFacingRight] =
        [
            Show(18, 1),
            Hold,
        ],
        [P.MountFacingDown] =
        [
            Show(19, 1),
            Hold,
        ],
        [P.MountFacingLeft] =
        [
            Show(20, 1),
            Hold,
        ],
        [P.MountFacingUp] =
        [
            Show(21, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue> GoTo(MorphBallEyeProgram program) => AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue> FallInto(MorphBallEyeProgram program) => AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue> Hold => AnimationProgramStep<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue>.Hold;

}

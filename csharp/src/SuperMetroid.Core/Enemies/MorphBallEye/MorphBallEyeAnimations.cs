using P = SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The Morph Ball eye's tracking, eyelid and mount animations as typed programs (#641). Each
/// frame shows a <see cref="MorphBallEyeFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class MorphBallEyeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MorphBallEyeProgram, MorphBallEyeFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MorphBallEyeProgram, AnimationBeat<MorphBallEyeFrame, NoAnimationCue>[]>
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
        ],
        [P.FacingRightDeactivating] =
        [
            Show(15, 8),
            Show(3, 48),
            Show(15, 5),
        ],
        [P.FacingRightClosed] =
        [
            Show(14, 48),
        ],
        [P.FacingLeftDeactivating] =
        [
            Show(17, 8),
            Show(10, 48),
            Show(17, 5),
        ],
        [P.FacingLeftClosed] =
        [
            Show(16, 48),
        ],
        [P.FacingRightActivating] =
        [
            Show(14, 32),
            Show(15, 5),
            Show(3, 48),
            Show(15, 8),
        ],
        [P.FacingLeftActivating] =
        [
            Show(16, 32),
            Show(17, 5),
            Show(10, 48),
            Show(17, 8),
        ],
        [P.MountFacingRight] =
        [
            Show(18, 1),
        ],
        [P.MountFacingDown] =
        [
            Show(19, 1),
        ],
        [P.MountFacingLeft] =
        [
            Show(20, 1),
        ],
        [P.MountFacingUp] =
        [
            Show(21, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MorphBallEyeProgram, Animation<MorphBallEyeProgram>> StartingAt { get; } = new Dictionary<MorphBallEyeProgram, Animation<MorphBallEyeProgram>>
    {
        [P.Active] = new([new(P.Active, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightDeactivating] = new([new(P.FacingRightDeactivating, 0, 3, 1), new(P.FacingRightClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingRightClosed] = new([new(P.FacingRightClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingLeftDeactivating] = new([new(P.FacingLeftDeactivating, 0, 3, 1), new(P.FacingLeftClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingLeftClosed] = new([new(P.FacingLeftClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingRightActivating] = new([new(P.FacingRightActivating, 0, 4, 1)], AnimationEnd.Holds),
        [P.FacingLeftActivating] = new([new(P.FacingLeftActivating, 0, 4, 1)], AnimationEnd.Holds),
        [P.MountFacingRight] = new([new(P.MountFacingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.MountFacingDown] = new([new(P.MountFacingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.MountFacingLeft] = new([new(P.MountFacingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.MountFacingUp] = new([new(P.MountFacingUp, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MorphBallEyeProgram> StartingWith(MorphBallEyeProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MorphBallEyeFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<MorphBallEyeFrame, NoAnimationCue>.Frame(new(pose), duration);
}

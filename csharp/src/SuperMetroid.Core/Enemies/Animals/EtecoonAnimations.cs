using P = SuperMetroid.Core.Enemies.Animals.EtecoonSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Etecoon's look, run, wall-jump, hop and flex animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class EtecoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EtecoonSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<EtecoonSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.LookRightAtSamusAndRunLeft] =
        [
            Show(EnemyFrameId.EtecoonLookRightAtSamusAndRunLeft, 5),
        ],
        [P.BeginRunningLeft] =
        [
            Show(EnemyFrameId.EtecoonBeginRunningLeft, 1),
        ],
        [P.RunningLeft] =
        [
            Show(EnemyFrameId.EtecoonRunningLeftFirst, 5),
            Show(EnemyFrameId.EtecoonRunningLeftSecond, 5),
            Show(EnemyFrameId.EtecoonRunningLeftThird, 5),
            Show(EnemyFrameId.EtecoonRunningLeftSecond, 5),
        ],
        [P.WallJumpLeft] =
        [
            Show(EnemyFrameId.EtecoonWallJumpLeft, 8),
        ],
        [P.WallJumpLeftLoop] =
        [
            Show(EnemyFrameId.EtecoonWallJumpLeftLoopFirst, 3),
            Show(EnemyFrameId.EtecoonWallJumpLeftLoopSecond, 3),
            Show(EnemyFrameId.EtecoonWallJumpLeftLoopThird, 3),
            Show(EnemyFrameId.EtecoonWallJumpLeftLoopFourth, 3),
        ],
        [P.HoppingFacingLeft] =
        [
            Show(EnemyFrameId.EtecoonHoppingFacingLeft, 1),
        ],
        [P.ContinueHoppingFacingLeft] =
        [
            Show(EnemyFrameId.EtecoonContinueHoppingFacingLeftFirst, 12),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingLeftSecond, 12),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingLeftThird, 6),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingLeftSecond, 12),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingLeftFirst, 12),
        ],
        [P.WallJumpLeftEligible] =
        [
            Show(EnemyFrameId.EtecoonWallJumpLeftEligible, 1),
        ],
        [P.LookLeftAtSamusAndRunRight] =
        [
            Show(EnemyFrameId.EtecoonLookLeftAtSamusAndRunRight, 5),
        ],
        [P.BeginRunningRight] =
        [
            Show(EnemyFrameId.EtecoonBeginRunningRight, 1),
        ],
        [P.RunningRight] =
        [
            Show(EnemyFrameId.EtecoonRunningRightFirst, 5),
            Show(EnemyFrameId.EtecoonRunningRightSecond, 5),
            Show(EnemyFrameId.EtecoonRunningRightThird, 5),
            Show(EnemyFrameId.EtecoonRunningRightSecond, 5),
        ],
        [P.WallJumpRight] =
        [
            Show(EnemyFrameId.EtecoonWallJumpRight, 8),
        ],
        [P.JumpingRight] =
        [
            Show(EnemyFrameId.EtecoonJumpingRightFirst, 3),
            Show(EnemyFrameId.EtecoonJumpingRightSecond, 3),
            Show(EnemyFrameId.EtecoonJumpingRightThird, 3),
            Show(EnemyFrameId.EtecoonJumpingRightFourth, 3),
        ],
        [P.HoppingFacingRight] =
        [
            Show(EnemyFrameId.EtecoonHoppingFacingRight, 1),
        ],
        [P.ContinueHoppingFacingRight] =
        [
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightFirst, 12),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightSecond, 12),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightThird, 6),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightSecond, 12),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightFirst, 12),
        ],
        [P.WallJumpRightEligible] =
        [
            Show(EnemyFrameId.EtecoonWallJumpRightEligible, 1),
        ],
        [P.Initial] =
        [
            Show(EnemyFrameId.EtecoonHoppingFacingRight, 8),
        ],
        [P.Flexing] =
        [
            Show(EnemyFrameId.EtecoonHoppingFacingRight, 8),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightFirst, 8),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightSecond, 8),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightThird, 8),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightSecond, 8),
            Show(EnemyFrameId.EtecoonContinueHoppingFacingRightFirst, 8),
            Show(EnemyFrameId.EtecoonFlexingLoopFifth, 32),
            Show(EnemyFrameId.EtecoonLookLeftAtSamusAndRunRight, 32),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EtecoonSegment, Animation<EtecoonSegment>> StartingAt { get; } = new Dictionary<EtecoonSegment, Animation<EtecoonSegment>>
    {
        [P.LookRightAtSamusAndRunLeft] = new([new(P.LookRightAtSamusAndRunLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.BeginRunningLeft] = new([new(P.BeginRunningLeft, 0, 1, 1), new(P.RunningLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningLeft] = new([new(P.RunningLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WallJumpLeft] = new([new(P.WallJumpLeft, 0, 1, 1), new(P.WallJumpLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WallJumpLeftLoop] = new([new(P.WallJumpLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HoppingFacingLeft] = new([new(P.HoppingFacingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.ContinueHoppingFacingLeft] = new([new(P.ContinueHoppingFacingLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.WallJumpLeftEligible] = new([new(P.WallJumpLeftEligible, 0, 1, 1)], AnimationEnd.Holds),
        [P.LookLeftAtSamusAndRunRight] = new([new(P.LookLeftAtSamusAndRunRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.BeginRunningRight] = new([new(P.BeginRunningRight, 0, 1, 1), new(P.RunningRight, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningRight] = new([new(P.RunningRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WallJumpRight] = new([new(P.WallJumpRight, 0, 1, 1), new(P.JumpingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.JumpingRight] = new([new(P.JumpingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HoppingFacingRight] = new([new(P.HoppingFacingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.ContinueHoppingFacingRight] = new([new(P.ContinueHoppingFacingRight, 0, 5, 1)], AnimationEnd.Holds),
        [P.WallJumpRightEligible] = new([new(P.WallJumpRightEligible, 0, 1, 1)], AnimationEnd.Holds),
        [P.Initial] = new([new(P.Initial, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The flex its wake starts (#2061): four plays of its six hop poses, then its two closing poses, held.</summary>
    internal static Animation<EtecoonSegment> Flexing { get; } = new([new(P.Flexing, 0, 6, 4), new(P.Flexing, 6, 2, 1)], AnimationEnd.Holds);

    /// <summary>
    /// The left hop entered at its crest (#2061), as hitting a ceiling enters it: the crest, then the way down.
    /// </summary>
    internal static AnimationPlayback<EtecoonSegment> LeftHopFromCrest { get; } = new(StartingAt[P.ContinueHoppingFacingLeft],
        Segments.PlaceOfFrame(StartingAt[P.ContinueHoppingFacingLeft], P.ContinueHoppingFacingLeft, EnemyFrameId.EtecoonContinueHoppingFacingLeftThird));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EtecoonSegment> StartingWith(EtecoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

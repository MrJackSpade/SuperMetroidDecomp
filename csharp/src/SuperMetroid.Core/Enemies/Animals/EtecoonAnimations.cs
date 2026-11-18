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
            Show(EnemyFrameId.EtecoonLookingRight, 5),
        ],
        [P.BeginRunningLeft] =
        [
            Show(EnemyFrameId.EtecoonTurningLeft, 1),
        ],
        [P.RunningLeft] =
        [
            Show(EnemyFrameId.EtecoonRunningLeftStretched, 5),
            Show(EnemyFrameId.EtecoonRunningLeftBunched, 5),
            Show(EnemyFrameId.EtecoonRunningLeftPushingOff, 5),
            Show(EnemyFrameId.EtecoonRunningLeftBunched, 5),
        ],
        [P.WallJumpLeft] =
        [
            Show(EnemyFrameId.EtecoonLeapingLeft, 8),
        ],
        [P.JumpingLeft] =
        [
            Show(EnemyFrameId.EtecoonSpinningLeftTurned0, 3),
            Show(EnemyFrameId.EtecoonSpinningLeftTurned90, 3),
            Show(EnemyFrameId.EtecoonSpinningLeftTurned180, 3),
            Show(EnemyFrameId.EtecoonSpinningLeftTurned270, 3),
        ],
        [P.HoppingFacingLeft] = [Hold(EnemyFrameId.EtecoonHopCrouchFacingLeft)],
        [P.ContinueHoppingFacingLeft] =
        [
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingLeft, 12),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingLeft, 12),
            Show(EnemyFrameId.EtecoonHopArmsFlungFacingLeft, 6),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingLeft, 12),
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingLeft, 12),
        ],
        [P.WallJumpLeftEligible] = [Hold(EnemyFrameId.EtecoonClingingFacingLeft)],
        [P.LookLeftAtSamusAndRunRight] =
        [
            Show(EnemyFrameId.EtecoonLookingLeft, 5),
        ],
        [P.BeginRunningRight] =
        [
            Show(EnemyFrameId.EtecoonTurningRight, 1),
        ],
        [P.RunningRight] =
        [
            Show(EnemyFrameId.EtecoonRunningRightStretched, 5),
            Show(EnemyFrameId.EtecoonRunningRightBunched, 5),
            Show(EnemyFrameId.EtecoonRunningRightPushingOff, 5),
            Show(EnemyFrameId.EtecoonRunningRightBunched, 5),
        ],
        [P.WallJumpRight] =
        [
            Show(EnemyFrameId.EtecoonLeapingRight, 8),
        ],
        [P.JumpingRight] =
        [
            Show(EnemyFrameId.EtecoonSpinningRightTurned0, 3),
            Show(EnemyFrameId.EtecoonSpinningRightTurned90, 3),
            Show(EnemyFrameId.EtecoonSpinningRightTurned180, 3),
            Show(EnemyFrameId.EtecoonSpinningRightTurned270, 3),
        ],
        [P.HoppingFacingRight] = [Hold(EnemyFrameId.EtecoonHopCrouchFacingRight)],
        [P.ContinueHoppingFacingRight] =
        [
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, 12),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingRight, 12),
            Show(EnemyFrameId.EtecoonHopArmsFlungFacingRight, 6),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingRight, 12),
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, 12),
        ],
        [P.WallJumpRightEligible] = [Hold(EnemyFrameId.EtecoonClingingFacingRight)],
        [P.StartingCrouch] = [Hold(EnemyFrameId.EtecoonHopCrouchFacingRight)],
        [P.Flexing] =
        [
            Show(EnemyFrameId.EtecoonHopCrouchFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsFlungFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, 8),
        ],
        [P.FlexEnd] =
        [
            Show(EnemyFrameId.EtecoonFacingViewer, 32),
            Show(EnemyFrameId.EtecoonLookingLeft, 32),
        ],
    });

    /// <summary>How many times a woken Etecoon's flex plays its six hop poses.</summary>
    private const int FlexRuns = 4;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275): a crouch, a cling and the
    /// placed Etecoon's crouch are still pictures, and a wall jump either way leaps and then spins for good.
    /// </summary>
    internal static IReadOnlyDictionary<EtecoonSegment, Animation<EtecoonSegment>> StartingAt { get; } = new Dictionary<EtecoonSegment, Animation<EtecoonSegment>>
    {
        [P.LookRightAtSamusAndRunLeft] = new([Segments.Whole(P.LookRightAtSamusAndRunLeft)], AnimationEnd.Holds),
        [P.BeginRunningLeft] = new([Segments.Whole(P.BeginRunningLeft), Segments.Whole(P.RunningLeft)], AnimationEnd.LoopsFrom(1)),
        [P.RunningLeft] = new([Segments.Whole(P.RunningLeft)], AnimationEnd.LoopsFrom(0)),
        [P.WallJumpLeft] = new([Segments.Whole(P.WallJumpLeft), Segments.Whole(P.JumpingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.HoppingFacingLeft] = new([Segments.Whole(P.HoppingFacingLeft)], AnimationEnd.Holds),
        [P.ContinueHoppingFacingLeft] = new([Segments.Whole(P.ContinueHoppingFacingLeft)], AnimationEnd.Holds),
        [P.WallJumpLeftEligible] = new([Segments.Whole(P.WallJumpLeftEligible)], AnimationEnd.Holds),
        [P.LookLeftAtSamusAndRunRight] = new([Segments.Whole(P.LookLeftAtSamusAndRunRight)], AnimationEnd.Holds),
        [P.BeginRunningRight] = new([Segments.Whole(P.BeginRunningRight), Segments.Whole(P.RunningRight)], AnimationEnd.LoopsFrom(1)),
        [P.RunningRight] = new([Segments.Whole(P.RunningRight)], AnimationEnd.LoopsFrom(0)),
        [P.WallJumpRight] = new([Segments.Whole(P.WallJumpRight), Segments.Whole(P.JumpingRight)], AnimationEnd.LoopsFrom(1)),
        [P.JumpingRight] = new([Segments.Whole(P.JumpingRight)], AnimationEnd.LoopsFrom(0)),
        [P.HoppingFacingRight] = new([Segments.Whole(P.HoppingFacingRight)], AnimationEnd.Holds),
        [P.ContinueHoppingFacingRight] = new([Segments.Whole(P.ContinueHoppingFacingRight)], AnimationEnd.Holds),
        [P.WallJumpRightEligible] = new([Segments.Whole(P.WallJumpRightEligible)], AnimationEnd.Holds),
        [P.StartingCrouch] = new([Segments.Whole(P.StartingCrouch)], AnimationEnd.Holds),
    };

    /// <summary>The flex its wake starts (#2061): four plays of its six hop poses, then its two closing poses, held.</summary>
    internal static Animation<EtecoonSegment> Flexing { get; } = new([Segments.Repeated(P.Flexing, FlexRuns), Segments.Whole(P.FlexEnd)], AnimationEnd.Holds);

    /// <summary>
    /// The left hop entered at its crest (#2061), as hitting a ceiling enters it: the crest, then the way down.
    /// </summary>
    internal static AnimationPlayback<EtecoonSegment> LeftHopFromCrest { get; } = new(StartingAt[P.ContinueHoppingFacingLeft],
        Segments.PlaceOfFrame(StartingAt[P.ContinueHoppingFacingLeft], P.ContinueHoppingFacingLeft, EnemyFrameId.EtecoonHopArmsFlungFacingLeft));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EtecoonSegment> StartingWith(EtecoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}

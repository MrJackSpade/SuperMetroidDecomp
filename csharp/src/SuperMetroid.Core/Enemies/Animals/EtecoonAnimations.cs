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
        [P.WallJumpLeftLoop] =
        [
            Show(EnemyFrameId.EtecoonSpinningLeftTurned0, 3),
            Show(EnemyFrameId.EtecoonSpinningLeftTurned90, 3),
            Show(EnemyFrameId.EtecoonSpinningLeftTurned180, 3),
            Show(EnemyFrameId.EtecoonSpinningLeftTurned270, 3),
        ],
        [P.HoppingFacingLeft] =
        [
            Show(EnemyFrameId.EtecoonHopCrouchFacingLeft, 1),
        ],
        [P.ContinueHoppingFacingLeft] =
        [
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingLeft, 12),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingLeft, 12),
            Show(EnemyFrameId.EtecoonHopArmsFlungFacingLeft, 6),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingLeft, 12),
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingLeft, 12),
        ],
        [P.WallJumpLeftEligible] =
        [
            Show(EnemyFrameId.EtecoonClingingFacingLeft, 1),
        ],
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
        [P.HoppingFacingRight] =
        [
            Show(EnemyFrameId.EtecoonHopCrouchFacingRight, 1),
        ],
        [P.ContinueHoppingFacingRight] =
        [
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, 12),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingRight, 12),
            Show(EnemyFrameId.EtecoonHopArmsFlungFacingRight, 6),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingRight, 12),
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, 12),
        ],
        [P.WallJumpRightEligible] =
        [
            Show(EnemyFrameId.EtecoonClingingFacingRight, 1),
        ],
        [P.Initial] =
        [
            Show(EnemyFrameId.EtecoonHopCrouchFacingRight, 8),
        ],
        [P.Flexing] =
        [
            Show(EnemyFrameId.EtecoonHopCrouchFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsFlungFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsUpFacingRight, 8),
            Show(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, 8),
            Show(EnemyFrameId.EtecoonFacingViewer, 32),
            Show(EnemyFrameId.EtecoonLookingLeft, 32),
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
        Segments.PlaceOfFrame(StartingAt[P.ContinueHoppingFacingLeft], P.ContinueHoppingFacingLeft, EnemyFrameId.EtecoonHopArmsFlungFacingLeft));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EtecoonSegment> StartingWith(EtecoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

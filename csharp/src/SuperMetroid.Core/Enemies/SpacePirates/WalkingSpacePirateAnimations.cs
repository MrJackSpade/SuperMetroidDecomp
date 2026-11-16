using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="WalkingSpacePirateCue"/> with the values it acts on.
/// </summary>
internal static class WalkingSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WalkingSpacePirateSegment, EnemyFrameId, WalkingSpacePirateCue> Segments { get; } = new(
        new Dictionary<WalkingSpacePirateSegment, AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>[]>
    {
        [P.FlinchFacingLeft] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.WalkingPirateFlinchLeft, 16),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.WalkingPirateFlinchRight, 16),
        ],
        [P.WalkingLeft] =
        [
            Cue(new WalkingSpacePirateCue.BeginHeading(WalkingSpacePirateAction.Walking, HorizontalDirection.Left)),
            Show(EnemyFrameId.WalkingPirateWalkingLeftFarLegBack, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftFarLegLifting, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftFarLegPassing, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftFarLegPlanting, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftNearLegBack, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftNearLegLifting, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftNearLegPassing, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftNearLegPlanting, 10),
        ],
        [P.FireLasersLeft] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.WalkingPirateStandingFacingLeft, 24),
            Show(EnemyFrameId.WalkingPirateClawsLiftingFacingLeft, 8),
            Show(EnemyFrameId.WalkingPirateClawsForwardFacingLeft, 8),
            Show(EnemyFrameId.WalkingPirateClawsRaisedFacingLeft, 8),
            Cue(new WalkingSpacePirateCue.FireLaser(HorizontalDirection.Left, LaserHeight: 8)),
            Show(EnemyFrameId.WalkingPirateClawsRaisedLeaningFacingLeft, 8),
            Cue(new WalkingSpacePirateCue.FireLaser(HorizontalDirection.Left, LaserHeight: 2)),
            Show(EnemyFrameId.WalkingPirateFiringFacingLeft, 24),
            Cue(new WalkingSpacePirateCue.FireLaser(HorizontalDirection.Left, LaserHeight: -8)),
            Show(EnemyFrameId.WalkingPirateClawsRaisedLeaningFacingLeft, 8),
            Show(EnemyFrameId.WalkingPirateClawsRaisedFacingLeft, 8),
            Show(EnemyFrameId.WalkingPirateClawsForwardFacingLeft, 8),
            Show(EnemyFrameId.WalkingPirateClawsLiftingFacingLeft, 8),
            Cue(new WalkingSpacePirateCue.Signal(K.ChooseMovement)),
        ],
        [P.LookingFacingLeft] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.WalkingPirateLookingAtViewerFacingLeft, 32),
            Show(EnemyFrameId.WalkingPirateStandingFacingLeftTwin, 10),
            Show(EnemyFrameId.WalkingPirateLookingAwayFacingLeft, 32),
            Show(EnemyFrameId.WalkingPirateStandingFacingLeftTwin, 10),
            Show(EnemyFrameId.WalkingPirateLookingAtViewerFacingLeft, 32),
            Show(EnemyFrameId.WalkingPirateFacingViewer, 8),
        ],
        [P.WalkingRight] =
        [
            Cue(new WalkingSpacePirateCue.BeginHeading(WalkingSpacePirateAction.Walking, HorizontalDirection.Right)),
            Show(EnemyFrameId.WalkingPirateWalkingRightFarLegBack, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightFarLegLifting, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightFarLegPassing, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightFarLegPlanting, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightNearLegBack, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightNearLegLifting, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightNearLegPassing, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightNearLegPlanting, 10),
        ],
        [P.FireLasersRight] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.WalkingPirateStandingFacingRight, 24),
            Show(EnemyFrameId.WalkingPirateClawsLiftingFacingRight, 8),
            Show(EnemyFrameId.WalkingPirateClawsForwardFacingRight, 8),
            Show(EnemyFrameId.WalkingPirateClawsRaisedFacingRight, 8),
            Cue(new WalkingSpacePirateCue.FireLaser(HorizontalDirection.Right, LaserHeight: 8)),
            Show(EnemyFrameId.WalkingPirateClawsRaisedLeaningFacingRight, 8),
            Cue(new WalkingSpacePirateCue.FireLaser(HorizontalDirection.Right, LaserHeight: 2)),
            Show(EnemyFrameId.WalkingPirateFiringFacingRight, 24),
            Cue(new WalkingSpacePirateCue.FireLaser(HorizontalDirection.Right, LaserHeight: -8)),
            Show(EnemyFrameId.WalkingPirateClawsRaisedLeaningFacingRight, 8),
            Show(EnemyFrameId.WalkingPirateClawsRaisedFacingRight, 8),
            Show(EnemyFrameId.WalkingPirateClawsForwardFacingRight, 8),
            Show(EnemyFrameId.WalkingPirateClawsLiftingFacingRight, 8),
            Cue(new WalkingSpacePirateCue.Signal(K.ChooseMovement)),
        ],
        [P.LookingFacingRight] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.WalkingPirateLookingAtViewerFacingRight, 32),
            Show(EnemyFrameId.WalkingPirateStandingFacingRightTwin, 10),
            Show(EnemyFrameId.WalkingPirateLookingAwayFacingRight, 32),
            Show(EnemyFrameId.WalkingPirateStandingFacingRightTwin, 10),
            Show(EnemyFrameId.WalkingPirateLookingAtViewerFacingRight, 32),
            Show(EnemyFrameId.WalkingPirateFacingViewer, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WalkingSpacePirateSegment, Animation<WalkingSpacePirateSegment>> StartingAt { get; } = new Dictionary<WalkingSpacePirateSegment, Animation<WalkingSpacePirateSegment>>
    {
        [P.FlinchFacingLeft] = new([new(P.FlinchFacingLeft, 0, 2, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeft, 1, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FlinchFacingRight] = new([new(P.FlinchFacingRight, 0, 2, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRight, 1, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.WalkingLeft] = new([new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeft, 1, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FireLasersLeft] = new([new(P.FireLasersLeft, 0, 15, 1), new(P.LookingFacingLeft, 0, 7, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRight, 1, 8, 1)], AnimationEnd.LoopsFrom(3)),
        [P.LookingFacingLeft] = new([new(P.LookingFacingLeft, 0, 7, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRight, 1, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.WalkingRight] = new([new(P.WalkingRight, 0, 1, 1), new(P.WalkingRight, 1, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FireLasersRight] = new([new(P.FireLasersRight, 0, 15, 1), new(P.LookingFacingRight, 0, 7, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeft, 1, 8, 1)], AnimationEnd.LoopsFrom(3)),
        [P.LookingFacingRight] = new([new(P.LookingFacingRight, 0, 7, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeft, 1, 8, 1)], AnimationEnd.LoopsFrom(2)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WalkingSpacePirateSegment> StartingWith(WalkingSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WalkingSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WalkingSpacePirateCue> Cue(WalkingSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>.Event(cue);
}

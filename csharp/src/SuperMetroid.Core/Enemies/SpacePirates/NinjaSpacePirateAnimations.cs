using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using K = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="NinjaSpacePirateCue"/> with the values it acts on.
/// </summary>
internal static class NinjaSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NinjaSpacePirateSegment, EnemyFrameId, NinjaSpacePirateCue> Segments { get; } = new(
        new Dictionary<NinjaSpacePirateSegment, AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[]>
    {
        [P.ClawAttackLeft] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateCrouchFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawDrawnBackFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedHighFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedOverheadFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawSwungForwardFacingLeft, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeft, 2),
            Show(EnemyFrameId.NinjaPirateClawsSweptDownFacingLeft, 2),
            Show(EnemyFrameId.NinjaPirateUprightClawsLoweredFacingLeft, 2),
            Show(EnemyFrameId.NinjaPirateCrouchFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawDrawnBackFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedHighFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedOverheadFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawSwungForwardFacingLeft, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-16, 8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeft, 2),
            Show(EnemyFrameId.NinjaPirateClawsSweptDownFacingLeft, 2),
            Show(EnemyFrameId.NinjaPirateUprightClawsLoweredFacingLeft, 2),
        ],
        [P.SpinJumpLeft] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateCrouchFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawDrawnBackFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedHighFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedOverheadFacingLeft, 5),
            Show(EnemyFrameId.NinjaPirateClawSwungForwardFacingLeft, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeft, 2),
            Show(EnemyFrameId.NinjaPirateClawsSweptDownFacingLeft, 2),
            Show(EnemyFrameId.NinjaPirateUprightClawsLoweredFacingLeft, 2),
            Cue(new NinjaSpacePirateCue.Signal(K.ResetSpinSpeed)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateCrouchFacingLeft, 8),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.SpinJumpRising, HorizontalDirection.Left)),
        ],
        [P.SpinJumpLeftSpinning] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(EnemyFrameId.NinjaPirateSpinningFacingLeftTurned0, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingLeftTurned45, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingLeftTurned90, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingLeftTurned135, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingLeftTurned180, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingLeftTurned225, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingLeftTurned270, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingLeftTurned315, 1),
        ],
        [P.ActiveFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.Active)),
        ],
        [P.ActiveFacingLeftStance] =
        [
            Show(EnemyFrameId.NinjaPirateReadyFacingLeftFeetClose, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingLeftFeetApart, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingLeftFeetWide, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingLeftFeetApart, 10),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Cue(new NinjaSpacePirateCue.Signal(K.StartActive)),
        ],
        [P.FlinchFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateFlinchFacingLeft, 16),
        ],
        [P.DivekickLeftJump] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateLeapCrouchFacingLeft, 8),
            Cue(new NinjaSpacePirateCue.Signal(K.StartDivekick)),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickJump, HorizontalDirection.Left)),
        ],
        [P.DivekickLeftJumpFlash] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(EnemyFrameId.NinjaPirateTuckedJumpFacingLeft, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(EnemyFrameId.NinjaPirateTuckedJumpFacingLeft, 4),
        ],
        [P.DivekickLeftDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickDive, HorizontalDirection.Left)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateDiveKickFacingLeft, 1),
        ],
        [P.WalkToLeftPost] =
        [
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickWalkToPost, HorizontalDirection.Left)),
        ],
        [P.RunningToLeftPost] =
        [
            Show(EnemyFrameId.NinjaPirateRunningLeftFarLegBack, 5),
            Show(EnemyFrameId.NinjaPirateRunningLeftFarLegLifting, 5),
            Show(EnemyFrameId.NinjaPirateRunningLeftFarLegPassing, 5),
            Show(EnemyFrameId.NinjaPirateRunningLeftFarLegPlanting, 5),
            Show(EnemyFrameId.NinjaPirateRunningLeftNearLegBack, 5),
            Show(EnemyFrameId.NinjaPirateRunningLeftNearLegLifting, 5),
            Show(EnemyFrameId.NinjaPirateRunningLeftNearLegPassing, 5),
            Show(EnemyFrameId.NinjaPirateRunningLeftNearLegPlanting, 5),
        ],
        [P.AwaitingSamusFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.AwaitingSamus)),
        ],
        [P.LookingAroundFacingLeft] =
        [
            Show(EnemyFrameId.NinjaPirateLookingAtViewerFacingLeft, 32),
            Show(EnemyFrameId.NinjaPirateStandingFacingLeft, 10),
            Show(EnemyFrameId.NinjaPirateLookingAwayFacingLeft, 32),
            Show(EnemyFrameId.NinjaPirateStandingFacingLeft, 10),
            Show(EnemyFrameId.NinjaPirateLookingAtViewerFacingLeft, 32),
        ],
        [P.LandFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateStandingFacingLeft, 4),
            Show(EnemyFrameId.NinjaPirateLookingAtViewerFacingLeft, 8),
            Show(EnemyFrameId.NinjaPirateFacingViewer, 4),
            Show(EnemyFrameId.NinjaPirateStandingFacingRight, 4),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.ReadyToDivekick)),
        ],
        [P.LandFacingLeftReady] =
        [
            Show(EnemyFrameId.NinjaPirateReadyFacingRightFeetClose, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingRightFeetApart, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingRightFeetWide, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingRightFeetApart, 10),
        ],
        [P.KickFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateKickChamberedFacingLeft, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateKickExtendingFacingLeft, 4),
            Show(EnemyFrameId.NinjaPirateKickExtendedFacingLeft, 32),
            Show(EnemyFrameId.NinjaPirateKickExtendingFacingLeft, 4),
        ],
        [P.ClawAttackRight] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateCrouchFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawDrawnBackFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedHighFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedOverheadFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawSwungForwardFacingRight, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingRight, 2),
            Show(EnemyFrameId.NinjaPirateClawsSweptDownFacingRight, 2),
            Show(EnemyFrameId.NinjaPirateUprightClawsLoweredFacingRight, 2),
            Show(EnemyFrameId.NinjaPirateCrouchFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawDrawnBackFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedHighFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedOverheadFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawSwungForwardFacingRight, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(16, 8))),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingRight, 2),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawsSweptDownFacingRight, 2),
            Show(EnemyFrameId.NinjaPirateUprightClawsLoweredFacingRight, 2),
        ],
        [P.SpinJumpRight] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateCrouchFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawDrawnBackFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedHighFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawRaisedOverheadFacingRight, 5),
            Show(EnemyFrameId.NinjaPirateClawSwungForwardFacingRight, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingRight, 2),
            Show(EnemyFrameId.NinjaPirateClawsSweptDownFacingRight, 2),
            Show(EnemyFrameId.NinjaPirateUprightClawsLoweredFacingRight, 2),
            Cue(new NinjaSpacePirateCue.Signal(K.ResetSpinSpeed)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateCrouchFacingRight, 8),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.SpinJumpRising, HorizontalDirection.Right)),
        ],
        [P.SpinJumpRightSpinning] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(EnemyFrameId.NinjaPirateSpinningFacingRightTurned0, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingRightTurned45, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingRightTurned90, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingRightTurned135, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingRightTurned180, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingRightTurned225, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingRightTurned270, 1),
            Show(EnemyFrameId.NinjaPirateSpinningFacingRightTurned315, 1),
        ],
        [P.ActiveFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.Active)),
        ],
        [P.ActiveFacingRightStance] =
        [
            Show(EnemyFrameId.NinjaPirateReadyFacingRightFeetClose, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingRightFeetApart, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingRightFeetWide, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingRightFeetApart, 10),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Cue(new NinjaSpacePirateCue.Signal(K.StartActive)),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateFlinchFacingRight, 16),
        ],
        [P.DivekickRightJump] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateLeapCrouchFacingRight, 8),
            Cue(new NinjaSpacePirateCue.Signal(K.StartDivekick)),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickJump, HorizontalDirection.Right)),
        ],
        [P.DivekickRightJumpFlash] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(EnemyFrameId.NinjaPirateTuckedJumpFacingRight, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(EnemyFrameId.NinjaPirateTuckedJumpFacingRight, 4),
        ],
        [P.DivekickRightDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickDive, HorizontalDirection.Right)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateDiveKickFacingRight, 1),
        ],
        [P.WalkToRightPost] =
        [
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickWalkToPost, HorizontalDirection.Right)),
        ],
        [P.RunningToRightPost] =
        [
            Show(EnemyFrameId.NinjaPirateRunningRightFarLegBack, 5),
            Show(EnemyFrameId.NinjaPirateRunningRightFarLegLifting, 5),
            Show(EnemyFrameId.NinjaPirateRunningRightFarLegPassing, 5),
            Show(EnemyFrameId.NinjaPirateRunningRightFarLegPlanting, 5),
            Show(EnemyFrameId.NinjaPirateRunningRightNearLegBack, 5),
            Show(EnemyFrameId.NinjaPirateRunningRightNearLegLifting, 5),
            Show(EnemyFrameId.NinjaPirateRunningRightNearLegPassing, 5),
            Show(EnemyFrameId.NinjaPirateRunningRightNearLegPlanting, 5),
        ],
        [P.AwaitingSamusFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.AwaitingSamus)),
        ],
        [P.LookingAroundFacingRight] =
        [
            Show(EnemyFrameId.NinjaPirateLookingAtViewerFacingRight, 32),
            Show(EnemyFrameId.NinjaPirateStandingFacingRight, 10),
            Show(EnemyFrameId.NinjaPirateLookingAwayFacingRight, 32),
            Show(EnemyFrameId.NinjaPirateStandingFacingRight, 10),
            Show(EnemyFrameId.NinjaPirateLookingAtViewerFacingRight, 32),
        ],
        [P.LandFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateStandingFacingRight, 4),
            Show(EnemyFrameId.NinjaPirateLookingAtViewerFacingRight, 8),
            Show(EnemyFrameId.NinjaPirateFacingViewer, 4),
            Show(EnemyFrameId.NinjaPirateStandingFacingLeft, 4),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.ReadyToDivekick)),
        ],
        [P.LandFacingRightReady] =
        [
            Show(EnemyFrameId.NinjaPirateReadyFacingLeftFeetClose, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingLeftFeetApart, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingLeftFeetWide, 10),
            Show(EnemyFrameId.NinjaPirateReadyFacingLeftFeetApart, 10),
        ],
        [P.KickFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateKickChamberedFacingRight, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateKickExtendingFacingRight, 4),
            Show(EnemyFrameId.NinjaPirateKickExtendedFacingRight, 32),
            Show(EnemyFrameId.NinjaPirateKickExtendingFacingRight, 4),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each segment played whole; a loop plays on
    /// from the entry before it (#2276).
    /// </summary>
    internal static IReadOnlyDictionary<NinjaSpacePirateSegment, Animation<NinjaSpacePirateSegment>> StartingAt { get; } = new Dictionary<NinjaSpacePirateSegment, Animation<NinjaSpacePirateSegment>>
    {
        [P.ClawAttackLeft] = Active([P.ClawAttackLeft], P.ActiveFacingLeft, P.ActiveFacingLeftStance),
        [P.SpinJumpLeft] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.SpinJumpLeft)], [Segments.Whole(P.SpinJumpLeftSpinning)]),
        [P.ActiveFacingLeft] = Active([], P.ActiveFacingLeft, P.ActiveFacingLeftStance),
        [P.FlinchFacingLeft] = Active([P.FlinchFacingLeft], P.ActiveFacingLeft, P.ActiveFacingLeftStance),
        [P.DivekickLeftJump] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.DivekickLeftJump)], [Segments.Whole(P.DivekickLeftJumpFlash)]),
        [P.DivekickLeftDive] = new([Segments.Whole(P.DivekickLeftDive)], AnimationEnd.Holds),
        [P.WalkToLeftPost] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.WalkToLeftPost)], [Segments.Whole(P.RunningToLeftPost)]),
        [P.AwaitingSamusFacingLeft] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.AwaitingSamusFacingLeft)], [Segments.Whole(P.LookingAroundFacingLeft)]),
        [P.LandFacingLeft] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.LandFacingLeft)], [Segments.Whole(P.LandFacingLeftReady)]),
        [P.KickFacingLeft] = Active([P.KickFacingLeft], P.ActiveFacingLeft, P.ActiveFacingLeftStance),
        [P.ClawAttackRight] = Active([P.ClawAttackRight], P.ActiveFacingRight, P.ActiveFacingRightStance),
        [P.SpinJumpRight] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.SpinJumpRight)], [Segments.Whole(P.SpinJumpRightSpinning)]),
        [P.ActiveFacingRight] = Active([], P.ActiveFacingRight, P.ActiveFacingRightStance),
        [P.FlinchFacingRight] = Active([P.FlinchFacingRight], P.ActiveFacingRight, P.ActiveFacingRightStance),
        [P.DivekickRightJump] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.DivekickRightJump)], [Segments.Whole(P.DivekickRightJumpFlash)]),
        [P.DivekickRightDive] = new([Segments.Whole(P.DivekickRightDive)], AnimationEnd.Holds),
        [P.WalkToRightPost] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.WalkToRightPost)], [Segments.Whole(P.RunningToRightPost)]),
        [P.AwaitingSamusFacingRight] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.AwaitingSamusFacingRight)], [Segments.Whole(P.LookingAroundFacingRight)]),
        [P.LandFacingRight] = Animation<NinjaSpacePirateSegment>.LeadingInto([Segments.Whole(P.LandFacingRight)], [Segments.Whole(P.LandFacingRightReady)]),
        [P.KickFacingRight] = Active([P.KickFacingRight], P.ActiveFacingRight, P.ActiveFacingRightStance),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NinjaSpacePirateSegment> StartingWith(NinjaSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>
    /// Plays each of <paramref name="before"/> whole, then the active stance's entry, <paramref name="entry"/>, once and its
    /// stance <paramref name="stance"/> for good (#2414).
    /// </summary>
    private static Animation<P> Active(IReadOnlyList<P> before, P entry, P stance) =>
        Animation<P>.LeadingInto([.. before.Select(Segments.Whole), Segments.Whole(entry)], [Segments.Whole(stance)]);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Cue(NinjaSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Event(cue);
}

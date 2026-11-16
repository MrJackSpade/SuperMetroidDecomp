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
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeftTwin, 2),
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
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeftTwin, 2),
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
            Show(EnemyFrameId.NinjaPirateClawsReadyFacingLeftTwin, 2),
            Show(EnemyFrameId.NinjaPirateClawsSweptDownFacingLeft, 2),
            Show(EnemyFrameId.NinjaPirateUprightClawsLoweredFacingLeft, 2),
            Cue(new NinjaSpacePirateCue.Signal(K.ResetSpeed)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateCrouchFacingLeft, 8),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.SpinJumpLeftRising)),
        ],
        [P.SpinJumpLeftLoop] =
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
        [P.ActiveFacingLeftLoop] =
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
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.DivekickLeftJump)),
        ],
        [P.DivekickLeftJumpLoop] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(EnemyFrameId.NinjaPirateTuckedJumpFacingLeft, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(EnemyFrameId.NinjaPirateTuckedJumpFacingLeft, 4),
        ],
        [P.DivekickLeftDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.DivekickLeftDive)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateDiveKickFacingLeft, 1),
        ],
        [P.WalkToLeftPost] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.DivekickLeftWalkToPost)),
        ],
        [P.WalkToLeftPostLoop] =
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
        [P.InitialFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.Initial)),
        ],
        [P.InitialFacingLeftLoop] =
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
        [P.LandFacingLeftLoop] =
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
            Cue(new NinjaSpacePirateCue.Signal(K.ResetSpeed)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(EnemyFrameId.NinjaPirateCrouchFacingRight, 8),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.SpinJumpRightRising)),
        ],
        [P.SpinJumpRightLoop] =
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
        [P.ActiveFacingRightLoop] =
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
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.DivekickRightJump)),
        ],
        [P.DivekickRightJumpLoop] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(EnemyFrameId.NinjaPirateTuckedJumpFacingRight, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(EnemyFrameId.NinjaPirateTuckedJumpFacingRight, 4),
        ],
        [P.DivekickRightDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.DivekickRightDive)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateDiveKickFacingRight, 1),
        ],
        [P.WalkToRightPost] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.DivekickRightWalkToPost)),
        ],
        [P.WalkToRightPostLoop] =
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
        [P.InitialFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.Initial)),
        ],
        [P.InitialFacingRightLoop] =
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
        [P.LandFacingRightLoop] =
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

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NinjaSpacePirateSegment, Animation<NinjaSpacePirateSegment>> StartingAt { get; } = new Dictionary<NinjaSpacePirateSegment, Animation<NinjaSpacePirateSegment>>
    {
        [P.ClawAttackLeft] = new([new(P.ClawAttackLeft, 0, 23, 1), new(P.ActiveFacingLeft, 0, 1, 1), new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.SpinJumpLeft] = new([new(P.SpinJumpLeft, 0, 16, 1), new(P.SpinJumpLeftLoop, 0, 9, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpinJumpLeftLoop] = new([new(P.SpinJumpLeftLoop, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ActiveFacingLeft] = new([new(P.ActiveFacingLeft, 0, 1, 1), new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ActiveFacingLeftLoop] = new([new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlinchFacingLeft] = new([new(P.FlinchFacingLeft, 0, 2, 1), new(P.ActiveFacingLeft, 0, 1, 1), new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.DivekickLeftJump] = new([new(P.DivekickLeftJump, 0, 4, 1), new(P.DivekickLeftJumpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DivekickLeftJumpLoop] = new([new(P.DivekickLeftJumpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DivekickLeftDive] = new([new(P.DivekickLeftDive, 0, 4, 1)], AnimationEnd.Holds),
        [P.WalkToLeftPost] = new([new(P.WalkToLeftPost, 0, 1, 1), new(P.WalkToLeftPostLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WalkToLeftPostLoop] = new([new(P.WalkToLeftPostLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.InitialFacingLeft] = new([new(P.InitialFacingLeft, 0, 1, 1), new(P.InitialFacingLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InitialFacingLeftLoop] = new([new(P.InitialFacingLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LandFacingLeft] = new([new(P.LandFacingLeft, 0, 7, 1), new(P.LandFacingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LandFacingLeftLoop] = new([new(P.LandFacingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KickFacingLeft] = new([new(P.KickFacingLeft, 0, 6, 1), new(P.ActiveFacingLeft, 0, 1, 1), new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.ClawAttackRight] = new([new(P.ClawAttackRight, 0, 23, 1), new(P.ActiveFacingRight, 0, 1, 1), new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.SpinJumpRight] = new([new(P.SpinJumpRight, 0, 16, 1), new(P.SpinJumpRightLoop, 0, 9, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpinJumpRightLoop] = new([new(P.SpinJumpRightLoop, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ActiveFacingRight] = new([new(P.ActiveFacingRight, 0, 1, 1), new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ActiveFacingRightLoop] = new([new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlinchFacingRight] = new([new(P.FlinchFacingRight, 0, 2, 1), new(P.ActiveFacingRight, 0, 1, 1), new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.DivekickRightJump] = new([new(P.DivekickRightJump, 0, 4, 1), new(P.DivekickRightJumpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DivekickRightJumpLoop] = new([new(P.DivekickRightJumpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DivekickRightDive] = new([new(P.DivekickRightDive, 0, 4, 1)], AnimationEnd.Holds),
        [P.WalkToRightPost] = new([new(P.WalkToRightPost, 0, 1, 1), new(P.WalkToRightPostLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WalkToRightPostLoop] = new([new(P.WalkToRightPostLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.InitialFacingRight] = new([new(P.InitialFacingRight, 0, 1, 1), new(P.InitialFacingRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InitialFacingRightLoop] = new([new(P.InitialFacingRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LandFacingRight] = new([new(P.LandFacingRight, 0, 7, 1), new(P.LandFacingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LandFacingRightLoop] = new([new(P.LandFacingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KickFacingRight] = new([new(P.KickFacingRight, 0, 6, 1), new(P.ActiveFacingRight, 0, 1, 1), new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NinjaSpacePirateSegment> StartingWith(NinjaSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Cue(NinjaSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Event(cue);
}

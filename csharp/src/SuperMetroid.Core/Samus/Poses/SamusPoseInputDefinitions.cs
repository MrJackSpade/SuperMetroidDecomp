using System.Collections.Frozen;
using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Native pose-to-input dispatch and selected-condition diagnostics; no stored transition rows.</summary>
internal static class SamusPoseInputDefinitions
{
    /// <summary>The lists in the later native graph section, whose rules <see cref="SamusPoseInputRulesLate"/> holds.</summary>
    internal static readonly FrozenSet<SamusPoseInputList> LaterLists = new[]
    {
        SamusPoseInputList.SpringBallGroundRightPoseList,
        SamusPoseInputList.SpringBallGroundLeftPoseList,
        SamusPoseInputList.SpringBallFallingRightPoseList,
        SamusPoseInputList.SpringBallFallingLeftPoseList,
        SamusPoseInputList.SpringBallJumpRightPoseList,
        SamusPoseInputList.SpringBallJumpLeftPoseList,
        SamusPoseInputList.UnusedPose63List,
        SamusPoseInputList.UnusedPose64List,
        SamusPoseInputList.UnusedPose65List,
        SamusPoseInputList.UnusedPose66List,
        SamusPoseInputList.WallJumpRightPoseList,
        SamusPoseInputList.WallJumpLeftPoseList,
        SamusPoseInputList.RanIntoWallRightPoseList,
        SamusPoseInputList.RanIntoWallLeftPoseList,
        SamusPoseInputList.NormalJumpGunExtendedRightPoseList,
        SamusPoseInputList.NormalJumpGunExtendedLeftPoseList,
        SamusPoseInputList.NormalJumpAimDownRightPoseList,
        SamusPoseInputList.NormalJumpAimDownLeftPoseList,
        SamusPoseInputList.UnmorphingTransitionRightPoseList,
        SamusPoseInputList.UnmorphingTransitionLeftPoseList,
        SamusPoseInputList.TurningRightToLeftPoseList,
        SamusPoseInputList.TurningLeftToRightPoseList,
        SamusPoseInputList.TurningRightToLeftAimUpPoseList,
        SamusPoseInputList.TurningLeftToRightAimUpPoseList,
        SamusPoseInputList.TurningRightToLeftAimDiagonalDownPoseList,
        SamusPoseInputList.TurningLeftToRightAimDiagonalDownPoseList,
        SamusPoseInputList.ShinesparkWindupRightPoseList,
        SamusPoseInputList.ShinesparkWindupLeftPoseList,
        SamusPoseInputList.FallingAimDownRightPoseList,
        SamusPoseInputList.FallingAimDownLeftPoseList,
        SamusPoseInputList.UnusedPoseDfList,
        SamusPoseInputList.DraygonGrabbedNeutralLeftPoseList,
        SamusPoseInputList.DraygonGrabbedNeutralRightPoseList,
        SamusPoseInputList.MovingRightGunExtendedPoseList,
        SamusPoseInputList.MovingLeftGunExtendedPoseList,
        SamusPoseInputList.FallingGunExtendedRightPoseList,
        SamusPoseInputList.FallingGunExtendedLeftPoseList,
        SamusPoseInputList.MoonwalkTurnJumpLeftPoseList,
        SamusPoseInputList.MoonwalkTurnJumpRightPoseList,
        SamusPoseInputList.MoonwalkTurnJumpAimUpLeftPoseList,
        SamusPoseInputList.MoonwalkTurnJumpAimUpRightPoseList,
        SamusPoseInputList.MoonwalkTurnJumpAimDownLeftPoseList,
        SamusPoseInputList.MoonwalkTurnJumpAimDownRightPoseList,
    }.ToFrozenSet();

    /// <summary>
    /// Each canonical chord button and the controller button that stands for it: the transition tables name the
    /// default action buttons (R and L aim, X shoots, A jumps) for their bindings.
    /// </summary>
    internal static readonly SamusPoseChordButton[] ChordButtons =
    [
        new(CanonicalPoseButtons.AimUp, ControllerButton.R),
        new(CanonicalPoseButtons.AimDown, ControllerButton.L),
        new(CanonicalPoseButtons.Shoot, ControllerButton.X),
        new(CanonicalPoseButtons.Jump, ControllerButton.A),
        new(CanonicalPoseButtons.Right, ControllerButton.Right),
        new(CanonicalPoseButtons.Left, ControllerButton.Left),
        new(CanonicalPoseButtons.Down, ControllerButton.Down),
        new(CanonicalPoseButtons.Up, ControllerButton.Up),
    ];
}

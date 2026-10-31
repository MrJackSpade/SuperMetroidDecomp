using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Poses.SamusAnimationDelayPrograms;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's pose animation programs: each frame's delay and the instruction that ends each run (#1473).</summary>
/// <remarks>
/// Independently reviewed for #1165 against the cartridge's pose animation data and interpreter, and typed for #1473: a
/// program is the runs of frames a pose animates by, numbered as the pose's artwork numbers its frames, and closed by
/// the instruction each run ends in. A program holds every run up to the next pose's program, so the frames the breathing,
/// drained, falling, spin and wall-jump animations reach by instruction or by a special handler's frame all lie in it.
/// These are gameplay cadence and transition definitions, not editable sprite artwork.
/// </remarks>
internal static class SamusAnimationDelayDefinitions
{
    /// <summary>The animation <see cref="SamusPoseId.MovingRightNormalPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MovingRightNormalAnimation { get; } = new(
    [
        new(0, [2, 3, 2, 3, 2, 3, 2, 3, 2, 3], new SamusAnimationRestart()),
        new(11, [.. Repeat(4, 6), 3, 4, 4, 3], new SamusAnimationRestart()),
        new(22, [10], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.StandingAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingAimUpRightAnimation { get; } = new([new(0, [2, 16], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkFacingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MoonwalkFacingLeftAnimation { get; } = new([new(0, [.. Repeat(16, 6)], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.NormalLandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalLandingRightAnimation { get; } = new([new(0, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalLandingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalLandingLeftAnimation { get; } = new([new(0, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingLeftNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.SpinLandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram SpinLandingRightAnimation { get; } = new([new(0, [3, 5, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.SpinLandingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram SpinLandingLeftAnimation { get; } = new([new(0, [3, 5, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingLeftNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram LandingAimUpRightAnimation { get; } = new([new(0, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram LandingAimUpLeftAnimation { get; } = new([new(0, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram LandingAimDiagonalUpRightAnimation { get; } = new([new(0, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimDiagonalUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram LandingAimDiagonalUpLeftAnimation { get; } = new([new(0, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimDiagonalDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram LandingAimDiagonalDownRightAnimation { get; } = new([new(0, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimDiagonalDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram LandingAimDiagonalDownLeftAnimation { get; } = new([new(0, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.DrainedCrouchingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram DrainedCrouchingRightAnimation { get; } = new(
    [
        new(0, [2, 2, 2, 16], new SamusAnimationInstallDrainedFall()),
        new(5, [1], new SamusAnimationRepeat(5)),
        new(8, [16, 16, 16, 16], new SamusAnimationRepeat(8)),
        new(14, [3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.DrainedCrouchingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram DrainedCrouchingLeftAnimation { get; } = new(
    [
        new(0, [2, 2, 16], new SamusAnimationInstallDrainedFall()),
        new(4, [1], new SamusAnimationRepeat(4)),
        new(7, [8, 16, 16, 16, 16], new SamusAnimationRepeat(8)),
        new(14, [16, 16, 16], new SamusAnimationTransition(SamusPoseId.FacingLeftNormalPose)),
        new(19, [16, 16, 16, 16, 16], new SamusAnimationRepeat(10)),
        new(26, [16], new SamusAnimationRepeat(10)),
        new(29, [16], new SamusAnimationRepeat(29)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.DrainedStandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram DrainedStandingRightAnimation { get; } = new(
    [
        new(0, [16, 16, 16, 16], new SamusAnimationRestart()),
        new(5, [3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.DrainedStandingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram DrainedStandingLeftAnimation { get; } = new(
    [
        new(0, [16, 16, 16, 16], new SamusAnimationRestart()),
        new(5, [3], new SamusAnimationTransition(SamusPoseId.FacingLeftNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.FacingRightNormalPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram FacingRightNormalAnimation { get; } = new(
    [
        new(0, [10, 10, 10, 10], new SamusAnimationLoopUnlessLowEnergy()),
        new(5, [8, 8, 8, 8], new SamusAnimationRepeat(5)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingRightAnimation { get; } = new(
    [
        new(0, [10, 10, 10, 10], new SamusAnimationLoopUnlessLowEnergy()),
        new(5, [8, 8, 8, 8], new SamusAnimationRepeat(5)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.XrayingStandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram XrayingStandingRightAnimation { get; } = new([new(0, [15, 15, 15, 15, 15], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.StandingAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingAimDiagonalUpRightAnimation { get; } = new([new(0, [16], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.GrappleStandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram GrappleStandingRightAnimation { get; } = new([new(0, [16], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPoseAC"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPoseACAnimation { get; } = new([new(0, [2, 16], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPoseAE"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPoseAEAnimation { get; } = new([new(0, [2, 16], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPoseB0"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPoseB0Animation { get; } = new([new(0, [2, 16], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.GrappleSwingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram GrappleSwingRightAnimation { get; } = new([new(0, [.. Repeat(8, 66)], new SamusAnimationRepeat(65))]);

    /// <summary>The animation <see cref="SamusPoseId.NeutralJumpTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NeutralJumpTransitionRightAnimation { get; } = new([new(0, [1], new SamusAnimationTransition(SamusPoseId.NeutralJumpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NeutralJumpTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NeutralJumpTransitionLeftAnimation { get; } = new([new(0, [1], new SamusAnimationTransition(SamusPoseId.NeutralJumpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpTransitionAimUpRightAnimation { get; } = new([new(0, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpTransitionAimUpLeftAnimation { get; } = new([new(0, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpTransitionAimDiagonalUpRightAnimation { get; } = new([new(0, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpTransitionAimDiagonalUpLeftAnimation { get; } = new([new(0, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpTransitionAimDiagonalDownRightAnimation { get; } = new([new(0, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpTransitionAimDiagonalDownLeftAnimation { get; } = new([new(0, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NeutralJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NeutralJumpRightAnimation { get; } = new([new(0, [3, 4, 4, 4, 4, 80], new SamusAnimationRepeat(5))]);

    /// <summary>The animation <see cref="SamusPoseId.DamageBoostLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram DamageBoostLeftAnimation { get; } = new([new(0, [8, .. Repeat(2, 9)], new SamusAnimationRepeat(9))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpAimUpRightAnimation { get; } = new([new(0, [2, 16], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpAimDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpAimDownRightAnimation { get; } = new([new(0, [2, 16], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpForwardRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpForwardRightAnimation { get; } = new([new(0, [2, 3], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpGunExtendedRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram NormalJumpGunExtendedRightAnimation { get; } = new([new(0, [2, 16], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.FallingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram FallingRightAnimation { get; } = new(
    [
        new(0, [8, 6, 6], new SamusAnimationRepeat(2)),
        new(5, [8, 16], new SamusAnimationRepeat(6)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.FallingGunExtendedRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram FallingGunExtendedRightAnimation { get; } = new(
    [
        new(0, [8, 6, 6], new SamusAnimationRepeat(2)),
        new(5, [8, 16], new SamusAnimationRepeat(6)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.FallingAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram FallingAimUpRightAnimation { get; } = new([new(0, [2, 16, 16], new SamusAnimationRepeat(2))]);

    /// <summary>The animation <see cref="SamusPoseId.FallingAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram FallingAimDiagonalUpRightAnimation { get; } = new(
    [
        new(0, [2], new SamusAnimationHold()),
        new(2, [16], new SamusAnimationRepeat(2)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.FallingAimDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram FallingAimDownRightAnimation { get; } = new([new(0, [2, 16], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.KnockbackRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram KnockbackRightAnimation { get; } = new(
    [
        new(0, [2, 16], new SamusAnimationRepeat(1)),
        new(4, [6, 6, 6, 8], new SamusAnimationRestart()),
        new(9, [8, 8], new SamusAnimationRestart()),
        new(12, [10], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.MorphBallGroundRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MorphBallGroundRightAnimation { get; } = new(
    [
        new(0, [.. Repeat(3, 8)], new SamusAnimationRestart()),
        new(9, [3], new SamusAnimationRepeat(0)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.SpinJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram SpinJumpRightAnimation { get; } = new(
    [
        new(0, [4, 3, 2, 3, 2, 3, 2, 3, 2], new SamusAnimationRepeat(1)),
        new(11, [8], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.SpaceJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram SpaceJumpRightAnimation { get; } = new(
    [
        new(0, [4, .. Repeat(1, 8)], new SamusAnimationRepeat(1)),
        new(11, [8], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.ScrewAttackRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram ScrewAttackRightAnimation { get; } = new(
    [
        new(0, [4, .. Repeat(1, 24)], new SamusAnimationRepeat(1)),
        new(27, [8], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingLeftNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftJumpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftJumpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpForwardLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightJumpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightJumpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpForwardRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftCrouchingAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightCrouchingAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftFallingPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftFallingAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightFallingPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightFallingAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftAimUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightAimUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftAimDiagonalDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftAimDiagonalDownAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightAimDiagonalDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightAimDiagonalDownAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftJumpAimUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightJumpAimUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftJumpAimDownAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightJumpAimDownAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftFallingAimUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightFallingAimUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftFallingAimDownAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightFallingAimDownAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftCrouchingAimUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightCrouchingAimUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftCrouchingAimDiagonalDownAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightCrouchingAimDiagonalDownAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftAimDiagonalUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightAimDiagonalUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftJumpAimDiagonalUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightJumpAimDiagonalUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftFallingAimDiagonalUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightFallingAimDiagonalUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningRightToLeftCrouchingAimDiagonalUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram TurningLeftToRightCrouchingAimDiagonalUpAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MoonwalkTurnJumpLeftAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MoonwalkTurnJumpRightAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MoonwalkTurnJumpAimUpLeftAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MoonwalkTurnJumpAimUpRightAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MoonwalkTurnJumpAimDownLeftAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MoonwalkTurnJumpAimDownRightAnimation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPoseC6"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPoseC6Animation { get; } = new([new(0, [2, 2, 2], new SamusAnimationTransition(SamusPoseId.DraygonGrabbedNeutralLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPose63"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPose63Animation { get; } = new([new(0, [4, 3], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPose64"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPose64Animation { get; } = new([new(0, [4, 3], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPose65"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPose65Animation { get; } = new([new(0, [3, .. Repeat(2, 8)], new SamusAnimationRepeat(1))]);

    /// <summary>The animation <see cref="SamusPoseId.WallJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram WallJumpRightAnimation { get; } = new(
    [
        new(0, [5, 5], new SamusAnimationWallJumpSelect(3, 13, 23)),
        new(3, [3, 2, 3, 2, 3, 2, 3, 2], new SamusAnimationRepeat(3)),
        new(13, [2, 1, 2, 1, 2, 1, 2, 1], new SamusAnimationRepeat(13)),
        new(23, [.. Repeat(1, 24)], new SamusAnimationRepeat(23)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingTransitionRightAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.CrouchingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingTransitionLeftAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.CrouchingLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MorphingTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MorphingTransitionRightAnimation { get; } = new([new(0, [3, 3], new SamusAnimationItemAirborneTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallFallingRightPose, SamusPoseId.SpringBallGroundRightPose, SamusPoseId.SpringBallFallingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MorphingTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram MorphingTransitionLeftAnimation { get; } = new([new(0, [3, 3], new SamusAnimationItemAirborneTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.MorphBallFallingLeftPose, SamusPoseId.SpringBallGroundLeftPose, SamusPoseId.SpringBallFallingLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPose39"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPose39Animation { get; } = new([new(0, [0], new SamusAnimationTransition(SamusPoseId.UnusedPose20))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPose3A"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPose3AAnimation { get; } = new([new(0, [0], new SamusAnimationTransition(SamusPoseId.UnusedPose42))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingTransitionRightAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingTransitionLeftAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.FacingLeftNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnmorphingTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnmorphingTransitionRightAnimation { get; } = new([new(0, [3, 3], new SamusAnimationTransition(SamusPoseId.CrouchingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnmorphingTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnmorphingTransitionLeftAnimation { get; } = new([new(0, [3, 3], new SamusAnimationTransition(SamusPoseId.CrouchingLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPose3F"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPose3FAnimation { get; } = new([new(0, [0], new SamusAnimationItemTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.SpringBallGroundRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPose40"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPose40Animation { get; } = new([new(0, [0], new SamusAnimationItemTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.SpringBallGroundLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPoseDb"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPoseDbAnimation { get; } = new([new(0, [3, 3, 3], new SamusAnimationItemAirborneTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallFallingRightPose, SamusPoseId.SpringBallGroundRightPose, SamusPoseId.SpringBallFallingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPoseDc"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPoseDcAnimation { get; } = new([new(0, [3, 3, 3], new SamusAnimationItemAirborneTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPoseDd"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPoseDdAnimation { get; } = new([new(0, [3, 3, 3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnusedPoseDe"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram UnusedPoseDeAnimation { get; } = new([new(0, [3, 3, 3], new SamusAnimationTransition(SamusPoseId.DraygonGrabbedNeutralLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingTransitionAimUpRightAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingTransitionAimUpLeftAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingTransitionAimDiagonalUpRightAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingTransitionAimDiagonalUpLeftAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingTransitionAimDiagonalDownRightAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrouchingTransitionAimDiagonalDownLeftAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingTransitionAimUpRightAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.StandingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingTransitionAimUpLeftAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.StandingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingTransitionAimDiagonalUpRightAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.StandingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingTransitionAimDiagonalUpLeftAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.StandingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingTransitionAimDiagonalDownRightAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.StandingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram StandingTransitionAimDiagonalDownLeftAnimation { get; } = new([new(0, [3], new SamusAnimationTransition(SamusPoseId.StandingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.DraygonGrabbedMovingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram DraygonGrabbedMovingLeftAnimation { get; } = new([new(0, [.. Repeat(6, 6)], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.ShinesparkHorizontalRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram ShinesparkHorizontalRightAnimation { get; } = new([new(0, [8], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.CrystalFlashRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrystalFlashRightAnimation { get; } = new(
    [
        new(0, [3, 3, 1, 1], new SamusAnimationRepeat(2)),
        new(6, [12, 12, 12, 12], new SamusAnimationRepeat(6)),
        new(12, [3, 3, 3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.CrystalFlashLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram CrystalFlashLeftAnimation { get; } = new(
    [
        new(0, [3, 3, 1, 1], new SamusAnimationRepeat(2)),
        new(6, [12, 12, 12, 12], new SamusAnimationRepeat(6)),
        new(12, [3, 3, 3], new SamusAnimationTransition(SamusPoseId.FacingLeftNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.DeathSequenceRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram DeathSequenceRightAnimation { get; } = new([new(0, [.. Repeat(2, 6)], new SamusAnimationRepeat(5))]);

    /// <summary>The animation <see cref="SamusPoseId.ForwardFacingPowerSuitPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.ProgramForPose"/> maps to it.</summary>
    internal static SamusAnimationProgram ForwardFacingPowerSuitAnimation { get; } = new(
    [
        new(0, [8], new SamusAnimationRestart()),
        new(2, [.. Repeat(3, 89), 51, 2, 2, 2, 48], new SamusAnimationRepeat(95)),
    ]);
}

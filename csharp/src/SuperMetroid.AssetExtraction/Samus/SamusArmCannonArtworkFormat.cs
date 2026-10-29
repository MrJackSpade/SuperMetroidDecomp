using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Bounded retail arm-cannon visual geometry, separate from open/close mechanics.</summary>
public static class SamusArmCannonArtworkFormat
{

    /// <summary>Supported placement JSON schema revision, checked with the fixed pose, descriptor, direction, and frame table dimensions.</summary>
    public const int Version = 3;
    /// <summary>Installed editable placement, OBJ-attribute, and tile-selector JSON filename, separate from cover pixels and animation mechanics.</summary>
    public const string JsonFileName = "samus-arm-cannon.json";
    /// <summary>Installed indexed PNG filename holding the twelve 8-by-8 cover characters in native source-identity order.</summary>
    public const string TileFileName = "samus-arm-cannon-tiles.png";
    /// <summary>The 608 installed bytes of the drawing window: the pose descriptors, then the cost table they over-read.</summary>
    public const int DrawingDataByteCount = 608;
    /// <summary>Four tile-selector slots per native aim direction: closed sentinel 0 and three successive nonclosed cover-art frames.</summary>
    public const int FramesPerDirection = 4;
    /// <summary>Installed cover tiles: three opening frames for each of the four artwork orientations.</summary>
    public const int CoverTileCount = 12;
    /// <summary>
    /// Where pose 0–252's stock drawing descriptor begins within the drawing data: $90:C7DF-$C9D8, ArmCannonDrawingData,
    /// named descriptor selection for the existing 253-pose installed domain.
    /// </summary>
    internal static int StockPoseDescriptor(int pose)
    {
        if ((uint)pose >= InstalledSamusBodyArtwork.PoseCount) throw new ArgumentOutOfRangeException(nameof(pose));
        return SamusPoseNumbering.FromNumber(pose) switch
        {
            SamusPoseId.ForwardFacingPowerSuitPose or
            SamusPoseId.ForwardFacingSuitedPose => ArmCannonDescriptorDefinitions.FacingForward,
            SamusPoseId.FacingRightNormalPose or
            SamusPoseId.UnusedPose47 or
            SamusPoseId.RanIntoWallRightPose or
            SamusPoseId.GrappleStandingRightPose or
            SamusPoseId.FiringLandingRightPose or
            SamusPoseId.DraygonGrabbedFiringRightPose => ArmCannonDescriptorDefinitions.FacingRight,
            SamusPoseId.FacingLeftNormalPose or
            SamusPoseId.UnusedPose48 or
            SamusPoseId.RanIntoWallLeftPose or
            SamusPoseId.GrappleStandingLeftPose or
            SamusPoseId.DraygonGrabbedFiringLeftPose or
            SamusPoseId.FiringLandingLeftPose => ArmCannonDescriptorDefinitions.FacingLeft,
            SamusPoseId.StandingAimUpRightPose => ArmCannonDescriptorDefinitions.FacingRightAimingUp,
            SamusPoseId.StandingAimUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftAimingUp,
            SamusPoseId.StandingAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusPoseId.RanIntoWallAimUpRightPose or
            SamusPoseId.LandingAimDiagonalUpRightPose or
            SamusPoseId.DraygonGrabbedAimUpRightPose or
            SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
            SamusPoseId.StandingTransitionAimDiagonalUpRightPose => ArmCannonDescriptorDefinitions.FacingRightAimingUpRight,
            SamusPoseId.StandingAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusPoseId.DraygonGrabbedAimUpLeftPose or
            SamusPoseId.RanIntoWallAimUpLeftPose or
            SamusPoseId.LandingAimDiagonalUpLeftPose or
            SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
            SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftAimingUpLeft,
            SamusPoseId.StandingAimDiagonalDownRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
            SamusPoseId.GrappleStandingDownRightPose or
            SamusPoseId.RanIntoWallAimDownRightPose or
            SamusPoseId.LandingAimDiagonalDownRightPose or
            SamusPoseId.DraygonGrabbedAimDownRightPose or
            SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
            SamusPoseId.StandingTransitionAimDiagonalDownRightPose => ArmCannonDescriptorDefinitions.FacingRightAimingDownRight,
            SamusPoseId.StandingAimDiagonalDownLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
            SamusPoseId.GrappleStandingDownLeftPose or
            SamusPoseId.DraygonGrabbedAimDownLeftPose or
            SamusPoseId.RanIntoWallAimDownLeftPose or
            SamusPoseId.LandingAimDiagonalDownLeftPose or
            SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
            SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => ArmCannonDescriptorDefinitions.FacingLeftAimingDownLeft,
            SamusPoseId.MovingRightNormalPose or
            SamusPoseId.MovingLeftNormalPose or
            SamusPoseId.RunningAimUpRightPose or
            SamusPoseId.RunningAimUpLeftPose or
            SamusPoseId.SpinJumpRightPose or
            SamusPoseId.SpinJumpLeftPose or
            SamusPoseId.SpaceJumpRightPose or
            SamusPoseId.SpaceJumpLeftPose or
            SamusPoseId.MorphBallGroundRightPose or
            SamusPoseId.MorphBallMovingRightPose or
            SamusPoseId.MorphBallMovingLeftPose or
            SamusPoseId.UnusedPose20 or
            SamusPoseId.UnusedPose21 or
            SamusPoseId.UnusedPose22 or
            SamusPoseId.UnusedPose23 or
            SamusPoseId.UnusedPose24 or
            SamusPoseId.TurningRightToLeftPose or
            SamusPoseId.TurningLeftToRightPose or
            SamusPoseId.FallingRightPose or
            SamusPoseId.FallingLeftPose or
            SamusPoseId.TurningRightToLeftJumpPose or
            SamusPoseId.TurningLeftToRightJumpPose or
            SamusPoseId.MorphBallFallingRightPose or
            SamusPoseId.MorphBallFallingLeftPose or
            SamusPoseId.UnusedKnockbackRightPose or
            SamusPoseId.UnusedKnockbackLeftPose or
            SamusPoseId.CrouchingTransitionRightPose or
            SamusPoseId.CrouchingTransitionLeftPose or
            SamusPoseId.MorphingTransitionRightPose or
            SamusPoseId.MorphingTransitionLeftPose or
            SamusPoseId.UnusedPose39 or
            SamusPoseId.UnusedPose3A or
            SamusPoseId.StandingTransitionRightPose or
            SamusPoseId.StandingTransitionLeftPose or
            SamusPoseId.UnmorphingTransitionRightPose or
            SamusPoseId.UnmorphingTransitionLeftPose or
            SamusPoseId.UnusedPose3F or
            SamusPoseId.UnusedPose40 or
            SamusPoseId.MorphBallGroundLeftPose or
            SamusPoseId.UnusedPose42 or
            SamusPoseId.TurningRightToLeftCrouchingPose or
            SamusPoseId.TurningLeftToRightCrouchingPose or
            SamusPoseId.UnusedPose45 or
            SamusPoseId.UnusedPose46 or
            SamusPoseId.NeutralJumpTransitionLeftPose or
            SamusPoseId.NeutralJumpRightPose or
            SamusPoseId.NeutralJumpLeftPose or
            SamusPoseId.DamageBoostLeftPose or
            SamusPoseId.DamageBoostRightPose or
            SamusPoseId.KnockbackRightPose or
            SamusPoseId.KnockbackLeftPose or
            SamusPoseId.UnusedPose5B or
            SamusPoseId.UnusedPose5C or
            SamusPoseId.UnusedPose5D or
            SamusPoseId.UnusedPose5E or
            SamusPoseId.UnusedPose5F or
            SamusPoseId.UnusedPose60 or
            SamusPoseId.UnusedPose61 or
            SamusPoseId.UnusedPose62 or
            SamusPoseId.UnusedPose63 or
            SamusPoseId.UnusedPose64 or
            SamusPoseId.UnusedPose65 or
            SamusPoseId.UnusedPose66 or
            SamusPoseId.SpringBallGroundRightPose or
            SamusPoseId.SpringBallGroundLeftPose or
            SamusPoseId.SpringBallMovingRightPose or
            SamusPoseId.SpringBallMovingLeftPose or
            SamusPoseId.SpringBallFallingRightPose or
            SamusPoseId.SpringBallFallingLeftPose or
            SamusPoseId.SpringBallJumpRightPose or
            SamusPoseId.SpringBallJumpLeftPose or
            SamusPoseId.ScrewAttackRightPose or
            SamusPoseId.ScrewAttackLeftPose or
            SamusPoseId.WallJumpRightPose or
            SamusPoseId.WallJumpLeftPose or
            SamusPoseId.TurningRightToLeftFallingPose or
            SamusPoseId.TurningLeftToRightFallingPose or
            SamusPoseId.TurningRightToLeftAimUpPose or
            SamusPoseId.TurningLeftToRightAimUpPose or
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
            SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
            SamusPoseId.TurningRightToLeftJumpAimUpPose or
            SamusPoseId.TurningLeftToRightJumpAimUpPose or
            SamusPoseId.TurningRightToLeftJumpAimDownPose or
            SamusPoseId.TurningLeftToRightJumpAimDownPose or
            SamusPoseId.TurningRightToLeftFallingAimUpPose or
            SamusPoseId.TurningLeftToRightFallingAimUpPose or
            SamusPoseId.TurningRightToLeftFallingAimDownPose or
            SamusPoseId.TurningLeftToRightFallingAimDownPose or
            SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
            SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
            SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
            SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
            SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
            SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
            SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
            SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
            SamusPoseId.NormalLandingLeftPose or
            SamusPoseId.SpinLandingLeftPose or
            SamusPoseId.GrappleSwingRightPose or
            SamusPoseId.GrappleSwingLeftPose or
            SamusPoseId.GrappleWallContactLeftPose or
            SamusPoseId.GrappleWallContactRightPose or
            SamusPoseId.DraygonGrabbedNeutralLeftPose or
            SamusPoseId.DraygonGrabbedMovingLeftPose or
            SamusPoseId.MoonwalkTurnJumpLeftPose or
            SamusPoseId.MoonwalkTurnJumpRightPose or
            SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
            SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
            SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
            SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
            SamusPoseId.UnusedPoseC5 or
            SamusPoseId.UnusedPoseC6 or
            SamusPoseId.ShinesparkWindupRightPose or
            SamusPoseId.ShinesparkWindupLeftPose or
            SamusPoseId.ShinesparkHorizontalRightPose or
            SamusPoseId.ShinesparkHorizontalLeftPose or
            SamusPoseId.ShinesparkVerticalRightPose or
            SamusPoseId.ShinesparkVerticalLeftPose or
            SamusPoseId.ShinesparkDiagonalRightPose or
            SamusPoseId.ShinesparkDiagonalLeftPose or
            SamusPoseId.CrystalFlashRightPose or
            SamusPoseId.CrystalFlashLeftPose or
            SamusPoseId.XrayingStandingRightPose or
            SamusPoseId.XrayingStandingLeftPose or
            SamusPoseId.DeathSequenceRightPose or
            SamusPoseId.DeathSequenceLeftPose or
            SamusPoseId.XrayingCrouchingRightPose or
            SamusPoseId.XrayingCrouchingLeftPose or
            SamusPoseId.UnusedPoseDb or
            SamusPoseId.UnusedPoseDc or
            SamusPoseId.UnusedPoseDd or
            SamusPoseId.UnusedPoseDe or
            SamusPoseId.UnusedPoseDf or
            SamusPoseId.DrainedCrouchingRightPose or
            SamusPoseId.DrainedCrouchingLeftPose or
            SamusPoseId.DrainedStandingRightPose or
            SamusPoseId.DrainedStandingLeftPose or
            SamusPoseId.DraygonGrabbedNeutralRightPose or
            SamusPoseId.DraygonGrabbedMovingRightPose => ArmCannonDescriptorDefinitions.Default,
            SamusPoseId.MovingRightGunExtendedPose => ArmCannonDescriptorDefinitions.MovingRightGunExtended,
            SamusPoseId.MovingLeftGunExtendedPose => ArmCannonDescriptorDefinitions.MovingLeftGunExtended,
            SamusPoseId.RunningAimDiagonalUpRightPose => ArmCannonDescriptorDefinitions.MovingRightAimingUpRight,
            SamusPoseId.RunningAimDiagonalUpLeftPose => ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft,
            SamusPoseId.RunningAimDiagonalDownRightPose => ArmCannonDescriptorDefinitions.MovingRightAimingDownRight,
            SamusPoseId.RunningAimDiagonalDownLeftPose => ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft,
            SamusPoseId.NormalJumpGunExtendedRightPose or
            SamusPoseId.UnusedPoseAC => ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt,
            SamusPoseId.NormalJumpGunExtendedLeftPose or
            SamusPoseId.UnusedPoseAD => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt,
            SamusPoseId.NormalJumpAimUpRightPose => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp,
            SamusPoseId.NormalJumpAimUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp,
            SamusPoseId.NormalJumpAimDownRightPose or
            SamusPoseId.UnusedPoseAE => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDown,
            SamusPoseId.NormalJumpAimDownLeftPose or
            SamusPoseId.UnusedPoseAF => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown,
            SamusPoseId.CrouchingRightPose or
            SamusPoseId.GrappleCrouchingRightPose => ArmCannonDescriptorDefinitions.FacingRightCrouching,
            SamusPoseId.CrouchingLeftPose or
            SamusPoseId.GrappleCrouchingLeftPose => ArmCannonDescriptorDefinitions.FacingLeftCrouching,
            SamusPoseId.FallingAimUpRightPose => ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp,
            SamusPoseId.FallingAimUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp,
            SamusPoseId.FallingAimDownRightPose => ArmCannonDescriptorDefinitions.FacingRightFallingAimingDown,
            SamusPoseId.FallingAimDownLeftPose => ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDown,
            SamusPoseId.MoonwalkFacingLeftPose => ArmCannonDescriptorDefinitions.FacingLeftMoonwalk,
            SamusPoseId.MoonwalkFacingRightPose => ArmCannonDescriptorDefinitions.FacingRightMoonwalk,
            SamusPoseId.NeutralJumpTransitionRightPose => ArmCannonDescriptorDefinitions.FacingRightNormalJumpTransition,
            SamusPoseId.NormalJumpForwardRightPose => ArmCannonDescriptorDefinitions.FacingRightNormalJumpMovingForward,
            SamusPoseId.NormalJumpForwardLeftPose => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward,
            SamusPoseId.NormalJumpTransitionAimUpRightPose or
            SamusPoseId.LandingAimUpRightPose or
            SamusPoseId.CrouchingTransitionAimUpRightPose or
            SamusPoseId.StandingTransitionAimUpRightPose => ArmCannonDescriptorDefinitions.FacingRightTransitionAimingUp,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose or
            SamusPoseId.LandingAimUpLeftPose or
            SamusPoseId.CrouchingTransitionAimUpLeftPose or
            SamusPoseId.StandingTransitionAimUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp,
            SamusPoseId.FallingGunExtendedRightPose => ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended,
            SamusPoseId.FallingGunExtendedLeftPose => ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended,
            SamusPoseId.NormalJumpAimDiagonalUpRightPose => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight,
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft,
            SamusPoseId.NormalJumpAimDiagonalDownRightPose or
            SamusPoseId.UnusedPoseB0 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight,
            SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
            SamusPoseId.UnusedPoseB1 => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft,
            SamusPoseId.FallingAimDiagonalUpRightPose => ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight,
            SamusPoseId.FallingAimDiagonalUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft,
            SamusPoseId.FallingAimDiagonalDownRightPose => ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight,
            SamusPoseId.FallingAimDiagonalDownLeftPose => ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft,
            SamusPoseId.CrouchingAimDiagonalUpRightPose => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUpRight,
            SamusPoseId.CrouchingAimDiagonalUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUpLeft,
            SamusPoseId.CrouchingAimDiagonalDownRightPose or
            SamusPoseId.GrappleCrouchingDownRightPose => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingDownRight,
            SamusPoseId.CrouchingAimDiagonalDownLeftPose or
            SamusPoseId.GrappleCrouchingDownLeftPose => ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingDownLeft,
            SamusPoseId.MoonwalkAimUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft,
            SamusPoseId.MoonwalkAimUpRightPose => ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight,
            SamusPoseId.MoonwalkAimDownLeftPose => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft,
            SamusPoseId.MoonwalkAimDownRightPose => ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight,
            SamusPoseId.CrouchingAimUpRightPose => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp,
            SamusPoseId.CrouchingAimUpLeftPose => ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp,
            SamusPoseId.NormalLandingRightPose => ArmCannonDescriptorDefinitions.FacingRightLandingFromNormalJump,
            SamusPoseId.SpinLandingRightPose => ArmCannonDescriptorDefinitions.FacingRightLandingFromSpinJump,
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => throw new ArgumentOutOfRangeException(nameof(pose)),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
    }

    /// <summary>$90:C9D9, ArmCannonDrawingData_Default: no cover is drawn.</summary>
    private const byte HiddenDrawingMode = 0;
    /// <summary>$90:C9DE and other visible descriptor mode bytes: draw the cover normally.</summary>
    private const byte NormalDrawingMode = 1;
    /// <summary>$90:C9DC, ArmCannonDrawingData_FacingForward: forward-facing cover mode.</summary>
    private const byte ForwardDrawingMode = 2;
    /// <summary>$90:CA05 and vertical-aim descriptors: initial diagonal selector changes to the next direction word after frame zero.</summary>
    private const byte FrameDependentDirectionFlag = 1 << 7;
    /// <summary>
    /// CostOfSBAsInPowerBombs, twelve word costs by beam index: the drawing window's final 24 bytes, which alias this
    /// independent mechanics owner and which the arm cannon's over-read reaches.
    /// </summary>
    private const int AdjacentCostByteCount = 24;

    /// <summary>Where the special beam attacks' cost table starts in the drawing window.</summary>
    internal const int AdjacentCostStart = DrawingDataByteCount - AdjacentCostByteCount;

    /// <summary>The pose descriptors, ordered by position in the window.</summary>
    private static readonly int[] DrawingsByPosition =
    [
        ArmCannonDescriptorDefinitions.Default,
        ArmCannonDescriptorDefinitions.FacingForward,
        ArmCannonDescriptorDefinitions.FacingRight,
        ArmCannonDescriptorDefinitions.FacingLeft,
        ArmCannonDescriptorDefinitions.FacingRightAimingUp,
        ArmCannonDescriptorDefinitions.FacingLeftAimingUp,
        ArmCannonDescriptorDefinitions.FacingRightAimingUpRight,
        ArmCannonDescriptorDefinitions.FacingLeftAimingUpLeft,
        ArmCannonDescriptorDefinitions.FacingRightAimingDownRight,
        ArmCannonDescriptorDefinitions.FacingLeftAimingDownLeft,
        ArmCannonDescriptorDefinitions.MovingRightGunExtended,
        ArmCannonDescriptorDefinitions.MovingLeftGunExtended,
        ArmCannonDescriptorDefinitions.MovingRightAimingUpRight,
        ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft,
        ArmCannonDescriptorDefinitions.MovingRightAimingDownRight,
        ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft,
        ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt,
        ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt,
        ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp,
        ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp,
        ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDown,
        ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown,
        ArmCannonDescriptorDefinitions.FacingRightNormalJumpTransition,
        ArmCannonDescriptorDefinitions.FacingRightNormalJumpMovingForward,
        ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward,
        ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight,
        ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft,
        ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight,
        ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft,
        ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended,
        ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended,
        ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp,
        ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp,
        ArmCannonDescriptorDefinitions.FacingRightFallingAimingDown,
        ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDown,
        ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight,
        ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft,
        ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight,
        ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft,
        ArmCannonDescriptorDefinitions.FacingRightCrouching,
        ArmCannonDescriptorDefinitions.FacingLeftCrouching,
        ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUpRight,
        ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUpLeft,
        ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingDownRight,
        ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingDownLeft,
        ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp,
        ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp,
        ArmCannonDescriptorDefinitions.FacingLeftMoonwalk,
        ArmCannonDescriptorDefinitions.FacingRightMoonwalk,
        ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft,
        ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight,
        ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft,
        ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight,
        ArmCannonDescriptorDefinitions.FacingRightLandingFromNormalJump,
        ArmCannonDescriptorDefinitions.FacingRightLandingFromSpinJump,
        ArmCannonDescriptorDefinitions.FacingRightTransitionAimingUp,
        ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp,
    ];

    /// <summary>Finds the descriptor whose bytes contain <paramref name="position"/>.</summary>
    private static bool TryLocateDrawing(int position, out int drawing, out int offset)
    {
        for (int index = DrawingsByPosition.Length - 1; index >= 0; index--)
        {
            if ((ushort)DrawingsByPosition[index] <= position)
            {
                drawing = DrawingsByPosition[index];
                offset = position - (ushort)drawing;
                return true;
            }
        }
        drawing = default;
        offset = 0;
        return false;
    }

    /// <summary>Calculates named descriptor controls and adjacent cost aliases; coordinate geometry is resolved separately from body artwork.</summary>
    internal static bool TryStockDrawingByte(int position, out int value)
    {
        if (position is >= AdjacentCostStart and < DrawingDataByteCount)
        {
            int costOffset = position - AdjacentCostStart;
            int cost = SamusComboMechanics.GetPowerBombCost(
                SamusBeamCombinations.FromTableIndex(costOffset / sizeof(ushort)));
            value = (byte)(cost >> ((costOffset & 1) * 8));
            return true;
        }
        if (!TryLocateDrawing(position, out int drawing, out int byteOffset))
        {
            value = 0;
            return false;
        }
        // Control bytes by descriptor and byte position; coordinate bytes are resolved separately.
        int? control = new ArmCannonControlByte(drawing, byteOffset) switch
        {
            (ArmCannonDescriptorDefinitions.FacingForward, 0) or
            (ArmCannonDescriptorDefinitions.Default, 0) => 0,
            (ArmCannonDescriptorDefinitions.FacingForward, 1) => ForwardDrawingMode,
            (ArmCannonDescriptorDefinitions.FacingRight, 0) or
            (ArmCannonDescriptorDefinitions.MovingRightGunExtended, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouching, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftMoonwalk, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpMovingForward, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended, 0) => SamusProjectileDirection.Right.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.FacingRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightAimingUp, 3) or
            (ArmCannonDescriptorDefinitions.FacingLeftAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftAimingUp, 3) or
            (ArmCannonDescriptorDefinitions.FacingRightAimingUpRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftAimingUpLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightAimingDownRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftAimingDownLeft, 1) or
            (ArmCannonDescriptorDefinitions.MovingRightGunExtended, 1) or
            (ArmCannonDescriptorDefinitions.MovingLeftGunExtended, 1) or
            (ArmCannonDescriptorDefinitions.MovingRightAimingUpRight, 1) or
            (ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft, 1) or
            (ArmCannonDescriptorDefinitions.MovingRightAimingDownRight, 1) or
            (ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp, 3) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp, 3) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDown, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouching, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouching, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp, 3) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp, 3) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingDown, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDown, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftMoonwalk, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightMoonwalk, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpTransition, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpMovingForward, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightTransitionAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUpRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUpLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingDownRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingDownLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp, 3) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp, 1) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp, 3) or
            (ArmCannonDescriptorDefinitions.FacingRightLandingFromNormalJump, 1) or
            (ArmCannonDescriptorDefinitions.FacingRightLandingFromSpinJump, 1) => NormalDrawingMode,
            (ArmCannonDescriptorDefinitions.FacingLeft, 0) or
            (ArmCannonDescriptorDefinitions.MovingLeftGunExtended, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouching, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightMoonwalk, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended, 0) => SamusProjectileDirection.Left.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.FacingRightAimingUp, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp, 0) => (byte)(FrameDependentDirectionFlag | SamusProjectileDirection.UpRight.ToAimIndex()),
            (ArmCannonDescriptorDefinitions.FacingRightAimingUp, 2) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp, 2) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp, 2) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp, 2) => (byte)(FrameDependentDirectionFlag | SamusProjectileDirection.UpFacingRight.ToAimIndex()),
            (ArmCannonDescriptorDefinitions.FacingLeftAimingUp, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp, 0) => (byte)(FrameDependentDirectionFlag | SamusProjectileDirection.UpLeft.ToAimIndex()),
            (ArmCannonDescriptorDefinitions.FacingLeftAimingUp, 2) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp, 2) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp, 2) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp, 2) => (byte)(FrameDependentDirectionFlag | SamusProjectileDirection.UpFacingLeft.ToAimIndex()),
            (ArmCannonDescriptorDefinitions.FacingRightAimingUpRight, 0) or
            (ArmCannonDescriptorDefinitions.MovingRightAimingUpRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUpRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight, 0) => SamusProjectileDirection.UpRight.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.FacingLeftAimingUpLeft, 0) or
            (ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUpLeft, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft, 0) => SamusProjectileDirection.UpLeft.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.FacingRightAimingDownRight, 0) or
            (ArmCannonDescriptorDefinitions.MovingRightAimingDownRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpTransition, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingDownRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightLandingFromNormalJump, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightLandingFromSpinJump, 0) => SamusProjectileDirection.DownRight.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.FacingLeftAimingDownLeft, 0) or
            (ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingDownLeft, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft, 0) => SamusProjectileDirection.DownLeft.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.Default, 1) => HiddenDrawingMode,
            (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDown, 0) or
            (ArmCannonDescriptorDefinitions.FacingRightFallingAimingDown, 0) => SamusProjectileDirection.DownFacingRight.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown, 0) or
            (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDown, 0) => SamusProjectileDirection.DownFacingLeft.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.FacingRightTransitionAimingUp, 0) => SamusProjectileDirection.UpFacingRight.ToAimIndex(),
            (ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp, 0) => SamusProjectileDirection.UpFacingLeft.ToAimIndex(),
            _ => null,
        };
        value = control ?? 0;
        return control.HasValue;
    }

    /// <summary>Repeated cover origins and fixed-X/repeated-Y running and moonwalking profiles.</summary>
    /// <remarks>Later stationary pairs, fixed X positions and repeated vertical cycles alias earlier coordinates. Dependencies strictly decrease in position; first-cycle defaults resolve through installed body geometry and the reviewed overlay joins.</remarks>
    internal static bool TryStockCoordinateSource(int position, out int source)
    {
        int firstPair = position switch
        {
            >= (ArmCannonDescriptorDefinitions.FacingRight + 4) and < ArmCannonDescriptorDefinitions.FacingLeft => ArmCannonDescriptorDefinitions.FacingRight + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeft + 4) and < ArmCannonDescriptorDefinitions.FacingRightAimingUp => ArmCannonDescriptorDefinitions.FacingLeft + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt + 4) and < ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt => ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt + 4) and < ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDown + 4) and < ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDown + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown + 4) and < ArmCannonDescriptorDefinitions.FacingRightNormalJumpTransition => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightNormalJumpMovingForward + 4) and < ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward => ArmCannonDescriptorDefinitions.FacingRightNormalJumpMovingForward + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward + 4) and < ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight + 4) and < ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft + 4) and < ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight + 4) and < ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft + 4) and < ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended => ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended + 4) and < ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended => ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended + 4) and < ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp => ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightFallingAimingDown + 4) and < ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDown => ArmCannonDescriptorDefinitions.FacingRightFallingAimingDown + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDown + 4) and < ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight => ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDown + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight + 4) and < ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft => ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft + 4) and < ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight => ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight + 4) and < ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft => ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft + 4) and < ArmCannonDescriptorDefinitions.FacingRightCrouching => ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightCrouching + 4) and < ArmCannonDescriptorDefinitions.FacingLeftCrouching => ArmCannonDescriptorDefinitions.FacingRightCrouching + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftCrouching + 4) and < ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUpRight => ArmCannonDescriptorDefinitions.FacingLeftCrouching + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightTransitionAimingUp + 4) and < ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp => ArmCannonDescriptorDefinitions.FacingRightTransitionAimingUp + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp + 4) and < AdjacentCostStart => ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp + 2,
            _ => -1,
        };
        if (firstPair >= 0)
        {
            source = firstPair + ((position - firstPair) & 1);
            return true;
        }
        // Running/moonwalking keeps its horizontal origin while the vertical
        // component follows separately supplied animation motion.
        int firstX = position switch
        {
            >= (ArmCannonDescriptorDefinitions.MovingRightGunExtended + 4) and < ArmCannonDescriptorDefinitions.MovingLeftGunExtended => ArmCannonDescriptorDefinitions.MovingRightGunExtended + 2,
            >= (ArmCannonDescriptorDefinitions.MovingLeftGunExtended + 4) and < ArmCannonDescriptorDefinitions.MovingRightAimingUpRight => ArmCannonDescriptorDefinitions.MovingLeftGunExtended + 2,
            >= (ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 4) and < ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft => ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 2,
            >= (ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 4) and < ArmCannonDescriptorDefinitions.MovingRightAimingDownRight => ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 2,
            >= (ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 4) and < ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft => ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 2,
            >= (ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 4) and < ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt => ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftMoonwalk + 4) and < ArmCannonDescriptorDefinitions.FacingRightMoonwalk => ArmCannonDescriptorDefinitions.FacingLeftMoonwalk + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightMoonwalk + 4) and < ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft => ArmCannonDescriptorDefinitions.FacingRightMoonwalk + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft + 4) and < ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight + 4) and < ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft => ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight + 2,
            >= (ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft + 4) and < ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft + 2,
            >= (ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight + 4) and < ArmCannonDescriptorDefinitions.FacingRightLandingFromNormalJump => ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight + 2,
            _ => -1,
        };
        bool fixedX = firstX >= 0 && ((position - firstX) & 1) == 0;
        if (fixedX)
        {
            source = firstX;
            return true;
        }
        // These vertical cycles repeat once per half of the native animation.
        // First-cycle positions resolve separately, preserving independent supplied edits.
        int cycleBytes = position switch
        {
            >= (ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 13) and < ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft => 10,
            >= (ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 13) and < ArmCannonDescriptorDefinitions.MovingRightAimingDownRight => 10,
            >= (ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 13) and < ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft => 10,
            >= (ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 13) and < ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt => 10,
            >= (ArmCannonDescriptorDefinitions.FacingLeftMoonwalk + 9) and < ArmCannonDescriptorDefinitions.FacingRightMoonwalk => 6,
            >= (ArmCannonDescriptorDefinitions.FacingRightMoonwalk + 9) and < ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft => 6,
            >= (ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft + 9) and < ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight => 6,
            >= (ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight + 9) and < ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft => 6,
            >= (ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft + 9) and < ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight => 6,
            >= (ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight + 9) and < ArmCannonDescriptorDefinitions.FacingRightLandingFromNormalJump => 6,
            _ => 0,
        };
        bool repeatedY = cycleBytes != 0 && firstX >= 0 && ((position - firstX) & 1) != 0;
        if (repeatedY)
        {
            source = position - cycleBytes;
            return true;
        }
        // Horizontal reflection preserves Y only in these exact native pose/frame
        // pairs. Other vertical profiles keep their independently chosen offsets.
        int pairedYSource = position switch
        {
            ArmCannonDescriptorDefinitions.FacingLeft + 3 => ArmCannonDescriptorDefinitions.FacingRight + 3,
            ArmCannonDescriptorDefinitions.FacingLeftAimingDownLeft + 3 => ArmCannonDescriptorDefinitions.FacingRightAimingDownRight + 3,
            ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 3 => ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 3,
            ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 5 => ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 5,
            ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 7 => ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 7,
            ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 9 => ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 9,
            ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 11 => ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 11,
            ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 3 => ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 3,
            ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 5 => ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 5,
            ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 7 => ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 7,
            ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 9 => ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 9,
            ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 11 => ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 11,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt + 3 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt + 3,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown + 3 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDown + 3,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward + 3 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpMovingForward + 3,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft + 3 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight + 3,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft + 3 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight + 3,
            ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended + 3 => ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended + 3,
            ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDown + 3 => ArmCannonDescriptorDefinitions.FacingRightFallingAimingDown + 3,
            ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft + 3 => ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight + 3,
            ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft + 3 => ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight + 3,
            ArmCannonDescriptorDefinitions.FacingLeftCrouching + 3 => ArmCannonDescriptorDefinitions.FacingRightCrouching + 3,
            ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingDownLeft + 3 => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingDownRight + 3,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalk + 3 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalk + 3,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalk + 5 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalk + 5,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalk + 7 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalk + 7,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight + 3 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft + 3,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight + 5 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft + 5,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight + 7 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft + 7,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight + 3 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft + 3,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight + 5 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft + 5,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight + 7 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft + 7,
            ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp + 3 => ArmCannonDescriptorDefinitions.FacingRightTransitionAimingUp + 3,
            ArmCannonDescriptorDefinitions.FacingLeftAimingUp + 7 => ArmCannonDescriptorDefinitions.FacingRightAimingUp + 7,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp + 7 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp + 7,
            ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp + 7 => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp + 7,
            _ => -1,
        };
        source = pairedYSource >= 0 ? pairedYSource : 0;
        return pairedYSource >= 0;
    }

    /// <summary>$90:C663 DrawArmCannon emits one small OBJ and transfers one32-byte4bpp tile: its horizontal footprint is eight pixels.</summary>
    private const int CoverWidthPixels = 8;

    /// <summary>Mirrors a signed cover X offset around Samus while accounting for its eight-pixel width.</summary>
    internal static byte ReflectCoverX(int coordinate) => unchecked((byte)(-unchecked((sbyte)coordinate) - CoverWidthPixels));

    /// <summary>Opposite-facing cover origins reflect the eight-pixel footprint around Samus's origin.</summary>
    /// <remarks>Only exact named facing pairs are included. Other coordinates resolve through
    /// the body attachment calculation, including asymmetric downward joins. Each source
    /// precedes its reflected result in the native window.</remarks>
    internal static bool TryStockReflectedXSource(int position, out int source)
    {
        int selected = position switch
        {
            ArmCannonDescriptorDefinitions.FacingLeft + 2 => ArmCannonDescriptorDefinitions.FacingRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftAimingUp + 4 => ArmCannonDescriptorDefinitions.FacingRightAimingUp + 4,
            ArmCannonDescriptorDefinitions.FacingLeftAimingUp + 6 => ArmCannonDescriptorDefinitions.FacingRightAimingUp + 6,
            ArmCannonDescriptorDefinitions.FacingLeftAimingUpLeft + 2 => ArmCannonDescriptorDefinitions.FacingRightAimingUpRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftAimingDownLeft + 2 => ArmCannonDescriptorDefinitions.FacingRightAimingDownRight + 2,
            ArmCannonDescriptorDefinitions.MovingLeftGunExtended + 2 => ArmCannonDescriptorDefinitions.MovingRightGunExtended + 2,
            ArmCannonDescriptorDefinitions.MovingLeftAimingUpLeft + 2 => ArmCannonDescriptorDefinitions.MovingRightAimingUpRight + 2,
            ArmCannonDescriptorDefinitions.MovingLeftAimingDownLeft + 2 => ArmCannonDescriptorDefinitions.MovingRightAimingDownRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt + 2 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpNotMovingGunExt + 2,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp + 4 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp + 4,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUp + 6 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUp + 6,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpMovingForward + 2 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpMovingForward + 2,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingUpLeft + 2 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingUpRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft + 2 => ArmCannonDescriptorDefinitions.FacingRightNormalJumpAimingDownRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftFallingGunExtended + 2 => ArmCannonDescriptorDefinitions.FacingRightFallingGunExtended + 2,
            ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp + 4 => ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp + 4,
            ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp + 6 => ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp + 6,
            ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUp + 8 => ArmCannonDescriptorDefinitions.FacingRightFallingAimingUp + 8,
            ArmCannonDescriptorDefinitions.FacingLeftFallingAimingUpLeft + 2 => ArmCannonDescriptorDefinitions.FacingRightFallingAimingUpRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftFallingAimingDownLeft + 2 => ArmCannonDescriptorDefinitions.FacingRightFallingAimingDownRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftCrouching + 2 => ArmCannonDescriptorDefinitions.FacingRightCrouching + 2,
            ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUpLeft + 2 => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUpRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingDownLeft + 2 => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingDownRight + 2,
            ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp + 4 => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp + 4,
            ArmCannonDescriptorDefinitions.FacingLeftCrouchingAimingUp + 6 => ArmCannonDescriptorDefinitions.FacingRightCrouchingAimingUp + 6,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalk + 2 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalk + 2,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingUpRight + 2 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingUpLeft + 2,
            ArmCannonDescriptorDefinitions.FacingRightMoonwalkAimingDownRight + 2 => ArmCannonDescriptorDefinitions.FacingLeftMoonwalkAimingDownLeft + 2,
            ArmCannonDescriptorDefinitions.FacingLeftTransitionAimingUp + 2 => ArmCannonDescriptorDefinitions.FacingRightTransitionAimingUp + 2,
            _ => -1,
        };
        source = selected >= 0 ? selected : 0;
        return selected >= 0;
    }

    /// <summary>Selects the arm-cannon tile source for vertical, horizontal, or diagonal aim.</summary>
    private enum TileOrientation
    {
        /// <summary>$90:C7B9 selects vertical frames from $9A:9A00.</summary>
        Vertical,
        /// <summary>$90:C7C1 selects horizontal frames from $9A:A000.</summary>
        Horizontal,
        /// <summary>$90:C7C9 selects downward-diagonal frames from $9A:A600.</summary>
        DownwardDiagonal,
        /// <summary>$90:C7D1 selects upward-diagonal frames from $9A:AC00.</summary>
        UpwardDiagonal,
    }
    /// <summary>$90:C791..C7A4 draws the cannon cover as sprite character $1F.</summary>
    private const int CoverCharacter = 31;
    /// <summary>$90:C791..C7A4 draws the cannon cover in palette row four.</summary>
    private const int CoverPalette = 4;
    /// <summary>$90:C791..C7A4 draws the cannon cover at priority two.</summary>
    private const int CoverPriority = 2;

    /// <summary>
    /// $90:C7A5..C7D8 chooses a physical artwork orientation and three successive opening frames: the installed cover
    /// tile opening <paramref name="frame"/> (1 through 3) shows.
    /// </summary>
    internal static int StockCoverTile(SamusProjectileDirection direction, int frame)
    {
        if (frame is < 1 or >= FramesPerDirection) throw new ArgumentOutOfRangeException(nameof(frame));
        TileOrientation orientation = direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.UpLeft => TileOrientation.UpwardDiagonal,
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => TileOrientation.Horizontal,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft => TileOrientation.DownwardDiagonal,
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft or
                SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => TileOrientation.Vertical,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined projectile direction."),
        };
        int orientationGroup = orientation switch
        {
            TileOrientation.Vertical => 0,
            TileOrientation.Horizontal => 1,
            TileOrientation.DownwardDiagonal => 2,
            TileOrientation.UpwardDiagonal => 3,
            _ => throw new InvalidOperationException($"Undefined {nameof(TileOrientation)} {orientation}."),
        };
        return orientationGroup * (FramesPerDirection - 1) + frame - 1;
    }

    /// <summary>$90:C791..C7A4 mirrors the selected vertical/horizontal/diagonal artwork according to native aiming direction.</summary>
    internal static SpritePart StockCoverAppearance(SamusProjectileDirection direction)
    {
        bool horizontal = direction is SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingLeft
            or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft;
        bool vertical = direction is SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft;
        return SpritePart.Single(CoverCharacter, CoverPalette, CoverPriority,
            (horizontal ? TileFlip.Horizontal : TileFlip.None) | (vertical ? TileFlip.Vertical : TileFlip.None));
    }
}

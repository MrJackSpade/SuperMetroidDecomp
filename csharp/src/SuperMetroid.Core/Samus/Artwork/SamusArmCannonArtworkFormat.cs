using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Bounded retail arm-cannon visual geometry, separate from open/close mechanics.</summary>
public static class SamusArmCannonArtworkFormat
{
    /// <summary>Where each arm-cannon drawing descriptor selected by Samus pose starts in the drawing window.</summary>
    internal static class ArmCannonDrawing
    {
        /// <summary>$90:C9DB, ArmCannonDrawingData_FacingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingForward = 0x002;
        /// <summary>$90:C9DD, ArmCannonDrawingData_FacingRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRight = 0x004;
        /// <summary>$90:C9F1, ArmCannonDrawingData_FacingLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeft = 0x018;
        /// <summary>$90:CA05, ArmCannonDrawingData_FacingRight_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightAimingUp = 0x02c;
        /// <summary>$90:CA0D, ArmCannonDrawingData_FacingLeft_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftAimingUp = 0x034;
        /// <summary>$90:CA15, ArmCannonDrawingData_FacingRight_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightAimingUpRight = 0x03c;
        /// <summary>$90:CA19, ArmCannonDrawingData_FacingLeft_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftAimingUpLeft = 0x040;
        /// <summary>$90:CA1D, ArmCannonDrawingData_FacingRight_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightAimingDownRight = 0x044;
        /// <summary>$90:CA21, ArmCannonDrawingData_FacingLeft_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftAimingDownLeft = 0x048;
        /// <summary>$90:C9D9, ArmCannonDrawingData_Default: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int Default = 0x000;
        /// <summary>$90:CA25, ArmCannonDrawingData_MovingRight_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int MovingRightGunExtended = 0x04c;
        /// <summary>$90:CA3B, ArmCannonDrawingData_MovingLeft_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int MovingLeftGunExtended = 0x062;
        /// <summary>$90:CA51, ArmCannonDrawingData_MovingRight_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int MovingRightAimingUpRight = 0x078;
        /// <summary>$90:CA67, ArmCannonDrawingData_MovingLeft_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int MovingLeftAimingUpLeft = 0x08e;
        /// <summary>$90:CA7D, ArmCannonDrawingData_MovingRight_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int MovingRightAimingDownRight = 0x0a4;
        /// <summary>$90:CA93, ArmCannonDrawingData_MovingLeft_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int MovingLeftAimingDownLeft = 0x0ba;
        /// <summary>$90:CAA9, ArmCannonDrawingData_FacingRight_NormalJump_NotMoving_GunExt: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightNormalJumpNotMovingGunExt = 0x0d0;
        /// <summary>$90:CAAF, ArmCannonDrawingData_FacingLeft_NormalJump_NotMoving_GunExt: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftNormalJumpNotMovingGunExt = 0x0d6;
        /// <summary>$90:CAB5, ArmCannonDrawingData_FacingRight_NormalJump_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightNormalJumpAimingUp = 0x0dc;
        /// <summary>$90:CABD, ArmCannonDrawingData_FacingLeft_NormalJump_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftNormalJumpAimingUp = 0x0e4;
        /// <summary>$90:CAC5, ArmCannonDrawingData_FacingRight_NormalJump_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightNormalJumpAimingDown = 0x0ec;
        /// <summary>$90:CACB, ArmCannonDrawingData_FacingLeft_NormalJump_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftNormalJumpAimingDown = 0x0f2;
        /// <summary>$90:CB5D, ArmCannonDrawingData_FacingRight_Crouching: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightCrouching = 0x184;
        /// <summary>$90:CB71, ArmCannonDrawingData_FacingLeft_Crouching: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftCrouching = 0x198;
        /// <summary>$90:CB1D, ArmCannonDrawingData_FacingRight_Falling_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightFallingAimingUp = 0x144;
        /// <summary>$90:CB27, ArmCannonDrawingData_FacingLeft_Falling_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftFallingAimingUp = 0x14e;
        /// <summary>$90:CB31, ArmCannonDrawingData_FacingRight_Falling_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightFallingAimingDown = 0x158;
        /// <summary>$90:CB37, ArmCannonDrawingData_FacingLeft_Falling_AimingDown: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftFallingAimingDown = 0x15e;
        /// <summary>$90:CBA5, ArmCannonDrawingData_FacingLeft_Moonwalk: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftMoonwalk = 0x1cc;
        /// <summary>$90:CBB3, ArmCannonDrawingData_FacingRight_Moonwalk: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightMoonwalk = 0x1da;
        /// <summary>$90:CAD1, ArmCannonDrawingData_FacingRight_NormalJumpTransition: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightNormalJumpTransition = 0x0f8;
        /// <summary>$90:CAD9, ArmCannonDrawingData_FacingRight_NormalJump_MovingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightNormalJumpMovingForward = 0x100;
        /// <summary>$90:CADF, ArmCannonDrawingData_FacingLeft_NormalJump_MovingForward: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftNormalJumpMovingForward = 0x106;
        /// <summary>$90:CC15, ArmCannonDrawingData_FacingRight_Transition_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightTransitionAimingUp = 0x23c;
        /// <summary>$90:CC1B, ArmCannonDrawingData_FacingLeft_Transition_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftTransitionAimingUp = 0x242;
        /// <summary>$90:CAFD, ArmCannonDrawingData_FacingRight_Falling_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightFallingGunExtended = 0x124;
        /// <summary>$90:CB0D, ArmCannonDrawingData_FacingLeft_Falling_GunExtended: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftFallingGunExtended = 0x134;
        /// <summary>$90:CAE5, ArmCannonDrawingData_FacingRight_NormalJump_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightNormalJumpAimingUpRight = 0x10c;
        /// <summary>$90:CAEB, ArmCannonDrawingData_FacingLeft_NormalJump_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftNormalJumpAimingUpLeft = 0x112;
        /// <summary>$90:CAF1, ArmCannonDrawingData_FacingRight_NormalJump_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightNormalJumpAimingDownRight = 0x118;
        /// <summary>$90:CAF7, ArmCannonDrawingData_FacingLeft_NormalJump_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftNormalJumpAimingDownLeft = 0x11e;
        /// <summary>$90:CB3D, ArmCannonDrawingData_FacingRight_Falling_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightFallingAimingUpRight = 0x164;
        /// <summary>$90:CB45, ArmCannonDrawingData_FacingLeft_Falling_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftFallingAimingUpLeft = 0x16c;
        /// <summary>$90:CB4D, ArmCannonDrawingData_FacingRight_Falling_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightFallingAimingDownRight = 0x174;
        /// <summary>$90:CB55, ArmCannonDrawingData_FacingLeft_Falling_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftFallingAimingDownLeft = 0x17c;
        /// <summary>$90:CB85, ArmCannonDrawingData_FacingRight_Crouching_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightCrouchingAimingUpRight = 0x1ac;
        /// <summary>$90:CB89, ArmCannonDrawingData_FacingLeft_Crouching_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftCrouchingAimingUpLeft = 0x1b0;
        /// <summary>$90:CB8D, ArmCannonDrawingData_FacingRight_Crouching_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightCrouchingAimingDownRight = 0x1b4;
        /// <summary>$90:CB91, ArmCannonDrawingData_FacingLeft_Crouching_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftCrouchingAimingDownLeft = 0x1b8;
        /// <summary>$90:CBC1, ArmCannonDrawingData_FacingLeft_Moonwalk_AimingUpLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftMoonwalkAimingUpLeft = 0x1e8;
        /// <summary>$90:CBCF, ArmCannonDrawingData_FacingRight_Moonwalk_AimingUpRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightMoonwalkAimingUpRight = 0x1f6;
        /// <summary>$90:CBDD, ArmCannonDrawingData_FacingLeft_Moonwalk_AimingDownLeft: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftMoonwalkAimingDownLeft = 0x204;
        /// <summary>$90:CBEB, ArmCannonDrawingData_FacingRight_Moonwalk_AimingDownRight: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightMoonwalkAimingDownRight = 0x212;
        /// <summary>$90:CB95, ArmCannonDrawingData_FacingRight_Crouching_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightCrouchingAimingUp = 0x1bc;
        /// <summary>$90:CB9D, ArmCannonDrawingData_FacingLeft_Crouching_AimingUp: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingLeftCrouchingAimingUp = 0x1c4;
        /// <summary>$90:CBF9, ArmCannonDrawingData_FacingRight_LandingFromNormalJump: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightLandingFromNormalJump = 0x220;
        /// <summary>$90:CC05, ArmCannonDrawingData_FacingRight_LandingFromSpinJump: native descriptor identity for the explicit pose cases below; drawing bytes remain independent.</summary>
        internal const int FacingRightLandingFromSpinJump = 0x22c;
    }

    /// <summary>Supported placement JSON schema revision, checked with the fixed pose, descriptor, direction, and frame table dimensions.</summary>
    public const int Version = 2;
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
        if ((uint)pose >= SamusBodyArtworkCatalog.PoseCount) throw new ArgumentOutOfRangeException(nameof(pose));
        return (SamusPoseId)pose switch
        {
            SamusPoseId.ForwardFacingPowerSuitPose or
            SamusPoseId.ForwardFacingSuitedPose => ArmCannonDrawing.FacingForward,
            SamusPoseId.FacingRightNormalPose or
            SamusPoseId.UnusedPose47 or
            SamusPoseId.RanIntoWallRightPose or
            SamusPoseId.GrappleStandingRightPose or
            SamusPoseId.FiringLandingRightPose or
            SamusPoseId.DraygonGrabbedFiringRightPose => ArmCannonDrawing.FacingRight,
            SamusPoseId.FacingLeftNormalPose or
            SamusPoseId.UnusedPose48 or
            SamusPoseId.RanIntoWallLeftPose or
            SamusPoseId.GrappleStandingLeftPose or
            SamusPoseId.DraygonGrabbedFiringLeftPose or
            SamusPoseId.FiringLandingLeftPose => ArmCannonDrawing.FacingLeft,
            SamusPoseId.StandingAimUpRightPose => ArmCannonDrawing.FacingRightAimingUp,
            SamusPoseId.StandingAimUpLeftPose => ArmCannonDrawing.FacingLeftAimingUp,
            SamusPoseId.StandingAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusPoseId.RanIntoWallAimUpRightPose or
            SamusPoseId.LandingAimDiagonalUpRightPose or
            SamusPoseId.DraygonGrabbedAimUpRightPose or
            SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
            SamusPoseId.StandingTransitionAimDiagonalUpRightPose => ArmCannonDrawing.FacingRightAimingUpRight,
            SamusPoseId.StandingAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusPoseId.DraygonGrabbedAimUpLeftPose or
            SamusPoseId.RanIntoWallAimUpLeftPose or
            SamusPoseId.LandingAimDiagonalUpLeftPose or
            SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
            SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => ArmCannonDrawing.FacingLeftAimingUpLeft,
            SamusPoseId.StandingAimDiagonalDownRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
            SamusPoseId.GrappleStandingDownRightPose or
            SamusPoseId.RanIntoWallAimDownRightPose or
            SamusPoseId.LandingAimDiagonalDownRightPose or
            SamusPoseId.DraygonGrabbedAimDownRightPose or
            SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
            SamusPoseId.StandingTransitionAimDiagonalDownRightPose => ArmCannonDrawing.FacingRightAimingDownRight,
            SamusPoseId.StandingAimDiagonalDownLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
            SamusPoseId.GrappleStandingDownLeftPose or
            SamusPoseId.DraygonGrabbedAimDownLeftPose or
            SamusPoseId.RanIntoWallAimDownLeftPose or
            SamusPoseId.LandingAimDiagonalDownLeftPose or
            SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
            SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => ArmCannonDrawing.FacingLeftAimingDownLeft,
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
            SamusPoseId.DraygonGrabbedMovingRightPose => ArmCannonDrawing.Default,
            SamusPoseId.MovingRightGunExtendedPose => ArmCannonDrawing.MovingRightGunExtended,
            SamusPoseId.MovingLeftGunExtendedPose => ArmCannonDrawing.MovingLeftGunExtended,
            SamusPoseId.RunningAimDiagonalUpRightPose => ArmCannonDrawing.MovingRightAimingUpRight,
            SamusPoseId.RunningAimDiagonalUpLeftPose => ArmCannonDrawing.MovingLeftAimingUpLeft,
            SamusPoseId.RunningAimDiagonalDownRightPose => ArmCannonDrawing.MovingRightAimingDownRight,
            SamusPoseId.RunningAimDiagonalDownLeftPose => ArmCannonDrawing.MovingLeftAimingDownLeft,
            SamusPoseId.NormalJumpGunExtendedRightPose or
            SamusPoseId.UnusedPoseAC => ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt,
            SamusPoseId.NormalJumpGunExtendedLeftPose or
            SamusPoseId.UnusedPoseAD => ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt,
            SamusPoseId.NormalJumpAimUpRightPose => ArmCannonDrawing.FacingRightNormalJumpAimingUp,
            SamusPoseId.NormalJumpAimUpLeftPose => ArmCannonDrawing.FacingLeftNormalJumpAimingUp,
            SamusPoseId.NormalJumpAimDownRightPose or
            SamusPoseId.UnusedPoseAE => ArmCannonDrawing.FacingRightNormalJumpAimingDown,
            SamusPoseId.NormalJumpAimDownLeftPose or
            SamusPoseId.UnusedPoseAF => ArmCannonDrawing.FacingLeftNormalJumpAimingDown,
            SamusPoseId.CrouchingRightPose or
            SamusPoseId.GrappleCrouchingRightPose => ArmCannonDrawing.FacingRightCrouching,
            SamusPoseId.CrouchingLeftPose or
            SamusPoseId.GrappleCrouchingLeftPose => ArmCannonDrawing.FacingLeftCrouching,
            SamusPoseId.FallingAimUpRightPose => ArmCannonDrawing.FacingRightFallingAimingUp,
            SamusPoseId.FallingAimUpLeftPose => ArmCannonDrawing.FacingLeftFallingAimingUp,
            SamusPoseId.FallingAimDownRightPose => ArmCannonDrawing.FacingRightFallingAimingDown,
            SamusPoseId.FallingAimDownLeftPose => ArmCannonDrawing.FacingLeftFallingAimingDown,
            SamusPoseId.MoonwalkFacingLeftPose => ArmCannonDrawing.FacingLeftMoonwalk,
            SamusPoseId.MoonwalkFacingRightPose => ArmCannonDrawing.FacingRightMoonwalk,
            SamusPoseId.NeutralJumpTransitionRightPose => ArmCannonDrawing.FacingRightNormalJumpTransition,
            SamusPoseId.NormalJumpForwardRightPose => ArmCannonDrawing.FacingRightNormalJumpMovingForward,
            SamusPoseId.NormalJumpForwardLeftPose => ArmCannonDrawing.FacingLeftNormalJumpMovingForward,
            SamusPoseId.NormalJumpTransitionAimUpRightPose or
            SamusPoseId.LandingAimUpRightPose or
            SamusPoseId.CrouchingTransitionAimUpRightPose or
            SamusPoseId.StandingTransitionAimUpRightPose => ArmCannonDrawing.FacingRightTransitionAimingUp,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose or
            SamusPoseId.LandingAimUpLeftPose or
            SamusPoseId.CrouchingTransitionAimUpLeftPose or
            SamusPoseId.StandingTransitionAimUpLeftPose => ArmCannonDrawing.FacingLeftTransitionAimingUp,
            SamusPoseId.FallingGunExtendedRightPose => ArmCannonDrawing.FacingRightFallingGunExtended,
            SamusPoseId.FallingGunExtendedLeftPose => ArmCannonDrawing.FacingLeftFallingGunExtended,
            SamusPoseId.NormalJumpAimDiagonalUpRightPose => ArmCannonDrawing.FacingRightNormalJumpAimingUpRight,
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose => ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft,
            SamusPoseId.NormalJumpAimDiagonalDownRightPose or
            SamusPoseId.UnusedPoseB0 => ArmCannonDrawing.FacingRightNormalJumpAimingDownRight,
            SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
            SamusPoseId.UnusedPoseB1 => ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft,
            SamusPoseId.FallingAimDiagonalUpRightPose => ArmCannonDrawing.FacingRightFallingAimingUpRight,
            SamusPoseId.FallingAimDiagonalUpLeftPose => ArmCannonDrawing.FacingLeftFallingAimingUpLeft,
            SamusPoseId.FallingAimDiagonalDownRightPose => ArmCannonDrawing.FacingRightFallingAimingDownRight,
            SamusPoseId.FallingAimDiagonalDownLeftPose => ArmCannonDrawing.FacingLeftFallingAimingDownLeft,
            SamusPoseId.CrouchingAimDiagonalUpRightPose => ArmCannonDrawing.FacingRightCrouchingAimingUpRight,
            SamusPoseId.CrouchingAimDiagonalUpLeftPose => ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft,
            SamusPoseId.CrouchingAimDiagonalDownRightPose or
            SamusPoseId.GrappleCrouchingDownRightPose => ArmCannonDrawing.FacingRightCrouchingAimingDownRight,
            SamusPoseId.CrouchingAimDiagonalDownLeftPose or
            SamusPoseId.GrappleCrouchingDownLeftPose => ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft,
            SamusPoseId.MoonwalkAimUpLeftPose => ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft,
            SamusPoseId.MoonwalkAimUpRightPose => ArmCannonDrawing.FacingRightMoonwalkAimingUpRight,
            SamusPoseId.MoonwalkAimDownLeftPose => ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft,
            SamusPoseId.MoonwalkAimDownRightPose => ArmCannonDrawing.FacingRightMoonwalkAimingDownRight,
            SamusPoseId.CrouchingAimUpRightPose => ArmCannonDrawing.FacingRightCrouchingAimingUp,
            SamusPoseId.CrouchingAimUpLeftPose => ArmCannonDrawing.FacingLeftCrouchingAimingUp,
            SamusPoseId.NormalLandingRightPose => ArmCannonDrawing.FacingRightLandingFromNormalJump,
            SamusPoseId.SpinLandingRightPose => ArmCannonDrawing.FacingRightLandingFromSpinJump,
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
        ArmCannonDrawing.Default,
        ArmCannonDrawing.FacingForward,
        ArmCannonDrawing.FacingRight,
        ArmCannonDrawing.FacingLeft,
        ArmCannonDrawing.FacingRightAimingUp,
        ArmCannonDrawing.FacingLeftAimingUp,
        ArmCannonDrawing.FacingRightAimingUpRight,
        ArmCannonDrawing.FacingLeftAimingUpLeft,
        ArmCannonDrawing.FacingRightAimingDownRight,
        ArmCannonDrawing.FacingLeftAimingDownLeft,
        ArmCannonDrawing.MovingRightGunExtended,
        ArmCannonDrawing.MovingLeftGunExtended,
        ArmCannonDrawing.MovingRightAimingUpRight,
        ArmCannonDrawing.MovingLeftAimingUpLeft,
        ArmCannonDrawing.MovingRightAimingDownRight,
        ArmCannonDrawing.MovingLeftAimingDownLeft,
        ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt,
        ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt,
        ArmCannonDrawing.FacingRightNormalJumpAimingUp,
        ArmCannonDrawing.FacingLeftNormalJumpAimingUp,
        ArmCannonDrawing.FacingRightNormalJumpAimingDown,
        ArmCannonDrawing.FacingLeftNormalJumpAimingDown,
        ArmCannonDrawing.FacingRightNormalJumpTransition,
        ArmCannonDrawing.FacingRightNormalJumpMovingForward,
        ArmCannonDrawing.FacingLeftNormalJumpMovingForward,
        ArmCannonDrawing.FacingRightNormalJumpAimingUpRight,
        ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft,
        ArmCannonDrawing.FacingRightNormalJumpAimingDownRight,
        ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft,
        ArmCannonDrawing.FacingRightFallingGunExtended,
        ArmCannonDrawing.FacingLeftFallingGunExtended,
        ArmCannonDrawing.FacingRightFallingAimingUp,
        ArmCannonDrawing.FacingLeftFallingAimingUp,
        ArmCannonDrawing.FacingRightFallingAimingDown,
        ArmCannonDrawing.FacingLeftFallingAimingDown,
        ArmCannonDrawing.FacingRightFallingAimingUpRight,
        ArmCannonDrawing.FacingLeftFallingAimingUpLeft,
        ArmCannonDrawing.FacingRightFallingAimingDownRight,
        ArmCannonDrawing.FacingLeftFallingAimingDownLeft,
        ArmCannonDrawing.FacingRightCrouching,
        ArmCannonDrawing.FacingLeftCrouching,
        ArmCannonDrawing.FacingRightCrouchingAimingUpRight,
        ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft,
        ArmCannonDrawing.FacingRightCrouchingAimingDownRight,
        ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft,
        ArmCannonDrawing.FacingRightCrouchingAimingUp,
        ArmCannonDrawing.FacingLeftCrouchingAimingUp,
        ArmCannonDrawing.FacingLeftMoonwalk,
        ArmCannonDrawing.FacingRightMoonwalk,
        ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft,
        ArmCannonDrawing.FacingRightMoonwalkAimingUpRight,
        ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft,
        ArmCannonDrawing.FacingRightMoonwalkAimingDownRight,
        ArmCannonDrawing.FacingRightLandingFromNormalJump,
        ArmCannonDrawing.FacingRightLandingFromSpinJump,
        ArmCannonDrawing.FacingRightTransitionAimingUp,
        ArmCannonDrawing.FacingLeftTransitionAimingUp,
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
    internal static bool TryStockDrawingByte(int position, out byte value)
    {
        if (position is >= AdjacentCostStart and < DrawingDataByteCount)
        {
            int costOffset = position - AdjacentCostStart;
            ushort cost = SamusComboMechanicsDefinitions.GetPowerBombCost(
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
        byte? control = new ArmCannonControlByte(drawing, byteOffset) switch
        {
            (ArmCannonDrawing.FacingForward, 0) or
            (ArmCannonDrawing.Default, 0) => 0,
            (ArmCannonDrawing.FacingForward, 1) => ForwardDrawingMode,
            (ArmCannonDrawing.FacingRight, 0) or
            (ArmCannonDrawing.MovingRightGunExtended, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt, 0) or
            (ArmCannonDrawing.FacingRightCrouching, 0) or
            (ArmCannonDrawing.FacingLeftMoonwalk, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpMovingForward, 0) or
            (ArmCannonDrawing.FacingRightFallingGunExtended, 0) => (byte)SamusProjectileDirection.Right,
            (ArmCannonDrawing.FacingRight, 1) or
            (ArmCannonDrawing.FacingLeft, 1) or
            (ArmCannonDrawing.FacingRightAimingUp, 1) or
            (ArmCannonDrawing.FacingRightAimingUp, 3) or
            (ArmCannonDrawing.FacingLeftAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftAimingUp, 3) or
            (ArmCannonDrawing.FacingRightAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightAimingDownRight, 1) or
            (ArmCannonDrawing.FacingLeftAimingDownLeft, 1) or
            (ArmCannonDrawing.MovingRightGunExtended, 1) or
            (ArmCannonDrawing.MovingLeftGunExtended, 1) or
            (ArmCannonDrawing.MovingRightAimingUpRight, 1) or
            (ArmCannonDrawing.MovingLeftAimingUpLeft, 1) or
            (ArmCannonDrawing.MovingRightAimingDownRight, 1) or
            (ArmCannonDrawing.MovingLeftAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUp, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUp, 3) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUp, 3) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingDown, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingDown, 1) or
            (ArmCannonDrawing.FacingRightCrouching, 1) or
            (ArmCannonDrawing.FacingLeftCrouching, 1) or
            (ArmCannonDrawing.FacingRightFallingAimingUp, 1) or
            (ArmCannonDrawing.FacingRightFallingAimingUp, 3) or
            (ArmCannonDrawing.FacingLeftFallingAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftFallingAimingUp, 3) or
            (ArmCannonDrawing.FacingRightFallingAimingDown, 1) or
            (ArmCannonDrawing.FacingLeftFallingAimingDown, 1) or
            (ArmCannonDrawing.FacingLeftMoonwalk, 1) or
            (ArmCannonDrawing.FacingRightMoonwalk, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpTransition, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpMovingForward, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpMovingForward, 1) or
            (ArmCannonDrawing.FacingRightTransitionAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftTransitionAimingUp, 1) or
            (ArmCannonDrawing.FacingRightFallingGunExtended, 1) or
            (ArmCannonDrawing.FacingLeftFallingGunExtended, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingDownRight, 1) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingRightFallingAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftFallingAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightFallingAimingDownRight, 1) or
            (ArmCannonDrawing.FacingLeftFallingAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightCrouchingAimingDownRight, 1) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft, 1) or
            (ArmCannonDrawing.FacingRightMoonwalkAimingUpRight, 1) or
            (ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft, 1) or
            (ArmCannonDrawing.FacingRightMoonwalkAimingDownRight, 1) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUp, 1) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUp, 3) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUp, 1) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUp, 3) or
            (ArmCannonDrawing.FacingRightLandingFromNormalJump, 1) or
            (ArmCannonDrawing.FacingRightLandingFromSpinJump, 1) => NormalDrawingMode,
            (ArmCannonDrawing.FacingLeft, 0) or
            (ArmCannonDrawing.MovingLeftGunExtended, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt, 0) or
            (ArmCannonDrawing.FacingLeftCrouching, 0) or
            (ArmCannonDrawing.FacingRightMoonwalk, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpMovingForward, 0) or
            (ArmCannonDrawing.FacingLeftFallingGunExtended, 0) => (byte)SamusProjectileDirection.Left,
            (ArmCannonDrawing.FacingRightAimingUp, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUp, 0) or
            (ArmCannonDrawing.FacingRightFallingAimingUp, 0) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUp, 0) => FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpRight,
            (ArmCannonDrawing.FacingRightAimingUp, 2) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUp, 2) or
            (ArmCannonDrawing.FacingRightFallingAimingUp, 2) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUp, 2) => FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpFacingRight,
            (ArmCannonDrawing.FacingLeftAimingUp, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUp, 0) or
            (ArmCannonDrawing.FacingLeftFallingAimingUp, 0) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUp, 0) => FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpLeft,
            (ArmCannonDrawing.FacingLeftAimingUp, 2) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUp, 2) or
            (ArmCannonDrawing.FacingLeftFallingAimingUp, 2) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUp, 2) => FrameDependentDirectionFlag | (byte)SamusProjectileDirection.UpFacingLeft,
            (ArmCannonDrawing.FacingRightAimingUpRight, 0) or
            (ArmCannonDrawing.MovingRightAimingUpRight, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingUpRight, 0) or
            (ArmCannonDrawing.FacingRightFallingAimingUpRight, 0) or
            (ArmCannonDrawing.FacingRightCrouchingAimingUpRight, 0) or
            (ArmCannonDrawing.FacingRightMoonwalkAimingUpRight, 0) => (byte)SamusProjectileDirection.UpRight,
            (ArmCannonDrawing.FacingLeftAimingUpLeft, 0) or
            (ArmCannonDrawing.MovingLeftAimingUpLeft, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft, 0) or
            (ArmCannonDrawing.FacingLeftFallingAimingUpLeft, 0) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft, 0) or
            (ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft, 0) => (byte)SamusProjectileDirection.UpLeft,
            (ArmCannonDrawing.FacingRightAimingDownRight, 0) or
            (ArmCannonDrawing.MovingRightAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpTransition, 0) or
            (ArmCannonDrawing.FacingRightNormalJumpAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightFallingAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightCrouchingAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightMoonwalkAimingDownRight, 0) or
            (ArmCannonDrawing.FacingRightLandingFromNormalJump, 0) or
            (ArmCannonDrawing.FacingRightLandingFromSpinJump, 0) => (byte)SamusProjectileDirection.DownRight,
            (ArmCannonDrawing.FacingLeftAimingDownLeft, 0) or
            (ArmCannonDrawing.MovingLeftAimingDownLeft, 0) or
            (ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft, 0) or
            (ArmCannonDrawing.FacingLeftFallingAimingDownLeft, 0) or
            (ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft, 0) or
            (ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft, 0) => (byte)SamusProjectileDirection.DownLeft,
            (ArmCannonDrawing.Default, 1) => HiddenDrawingMode,
            (ArmCannonDrawing.FacingRightNormalJumpAimingDown, 0) or
            (ArmCannonDrawing.FacingRightFallingAimingDown, 0) => (byte)SamusProjectileDirection.DownFacingRight,
            (ArmCannonDrawing.FacingLeftNormalJumpAimingDown, 0) or
            (ArmCannonDrawing.FacingLeftFallingAimingDown, 0) => (byte)SamusProjectileDirection.DownFacingLeft,
            (ArmCannonDrawing.FacingRightTransitionAimingUp, 0) => (byte)SamusProjectileDirection.UpFacingRight,
            (ArmCannonDrawing.FacingLeftTransitionAimingUp, 0) => (byte)SamusProjectileDirection.UpFacingLeft,
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
            >= (ArmCannonDrawing.FacingRight + 4) and < ArmCannonDrawing.FacingLeft => ArmCannonDrawing.FacingRight + 2,
            >= (ArmCannonDrawing.FacingLeft + 4) and < ArmCannonDrawing.FacingRightAimingUp => ArmCannonDrawing.FacingLeft + 2,
            >= (ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt + 4) and < ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt => ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt + 2,
            >= (ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt + 4) and < ArmCannonDrawing.FacingRightNormalJumpAimingUp => ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt + 2,
            >= (ArmCannonDrawing.FacingRightNormalJumpAimingDown + 4) and < ArmCannonDrawing.FacingLeftNormalJumpAimingDown => ArmCannonDrawing.FacingRightNormalJumpAimingDown + 2,
            >= (ArmCannonDrawing.FacingLeftNormalJumpAimingDown + 4) and < ArmCannonDrawing.FacingRightNormalJumpTransition => ArmCannonDrawing.FacingLeftNormalJumpAimingDown + 2,
            >= (ArmCannonDrawing.FacingRightNormalJumpMovingForward + 4) and < ArmCannonDrawing.FacingLeftNormalJumpMovingForward => ArmCannonDrawing.FacingRightNormalJumpMovingForward + 2,
            >= (ArmCannonDrawing.FacingLeftNormalJumpMovingForward + 4) and < ArmCannonDrawing.FacingRightNormalJumpAimingUpRight => ArmCannonDrawing.FacingLeftNormalJumpMovingForward + 2,
            >= (ArmCannonDrawing.FacingRightNormalJumpAimingUpRight + 4) and < ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft => ArmCannonDrawing.FacingRightNormalJumpAimingUpRight + 2,
            >= (ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft + 4) and < ArmCannonDrawing.FacingRightNormalJumpAimingDownRight => ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft + 2,
            >= (ArmCannonDrawing.FacingRightNormalJumpAimingDownRight + 4) and < ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft => ArmCannonDrawing.FacingRightNormalJumpAimingDownRight + 2,
            >= (ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft + 4) and < ArmCannonDrawing.FacingRightFallingGunExtended => ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft + 2,
            >= (ArmCannonDrawing.FacingRightFallingGunExtended + 4) and < ArmCannonDrawing.FacingLeftFallingGunExtended => ArmCannonDrawing.FacingRightFallingGunExtended + 2,
            >= (ArmCannonDrawing.FacingLeftFallingGunExtended + 4) and < ArmCannonDrawing.FacingRightFallingAimingUp => ArmCannonDrawing.FacingLeftFallingGunExtended + 2,
            >= (ArmCannonDrawing.FacingRightFallingAimingDown + 4) and < ArmCannonDrawing.FacingLeftFallingAimingDown => ArmCannonDrawing.FacingRightFallingAimingDown + 2,
            >= (ArmCannonDrawing.FacingLeftFallingAimingDown + 4) and < ArmCannonDrawing.FacingRightFallingAimingUpRight => ArmCannonDrawing.FacingLeftFallingAimingDown + 2,
            >= (ArmCannonDrawing.FacingRightFallingAimingUpRight + 4) and < ArmCannonDrawing.FacingLeftFallingAimingUpLeft => ArmCannonDrawing.FacingRightFallingAimingUpRight + 2,
            >= (ArmCannonDrawing.FacingLeftFallingAimingUpLeft + 4) and < ArmCannonDrawing.FacingRightFallingAimingDownRight => ArmCannonDrawing.FacingLeftFallingAimingUpLeft + 2,
            >= (ArmCannonDrawing.FacingRightFallingAimingDownRight + 4) and < ArmCannonDrawing.FacingLeftFallingAimingDownLeft => ArmCannonDrawing.FacingRightFallingAimingDownRight + 2,
            >= (ArmCannonDrawing.FacingLeftFallingAimingDownLeft + 4) and < ArmCannonDrawing.FacingRightCrouching => ArmCannonDrawing.FacingLeftFallingAimingDownLeft + 2,
            >= (ArmCannonDrawing.FacingRightCrouching + 4) and < ArmCannonDrawing.FacingLeftCrouching => ArmCannonDrawing.FacingRightCrouching + 2,
            >= (ArmCannonDrawing.FacingLeftCrouching + 4) and < ArmCannonDrawing.FacingRightCrouchingAimingUpRight => ArmCannonDrawing.FacingLeftCrouching + 2,
            >= (ArmCannonDrawing.FacingRightTransitionAimingUp + 4) and < ArmCannonDrawing.FacingLeftTransitionAimingUp => ArmCannonDrawing.FacingRightTransitionAimingUp + 2,
            >= (ArmCannonDrawing.FacingLeftTransitionAimingUp + 4) and < AdjacentCostStart => ArmCannonDrawing.FacingLeftTransitionAimingUp + 2,
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
            >= (ArmCannonDrawing.MovingRightGunExtended + 4) and < ArmCannonDrawing.MovingLeftGunExtended => ArmCannonDrawing.MovingRightGunExtended + 2,
            >= (ArmCannonDrawing.MovingLeftGunExtended + 4) and < ArmCannonDrawing.MovingRightAimingUpRight => ArmCannonDrawing.MovingLeftGunExtended + 2,
            >= (ArmCannonDrawing.MovingRightAimingUpRight + 4) and < ArmCannonDrawing.MovingLeftAimingUpLeft => ArmCannonDrawing.MovingRightAimingUpRight + 2,
            >= (ArmCannonDrawing.MovingLeftAimingUpLeft + 4) and < ArmCannonDrawing.MovingRightAimingDownRight => ArmCannonDrawing.MovingLeftAimingUpLeft + 2,
            >= (ArmCannonDrawing.MovingRightAimingDownRight + 4) and < ArmCannonDrawing.MovingLeftAimingDownLeft => ArmCannonDrawing.MovingRightAimingDownRight + 2,
            >= (ArmCannonDrawing.MovingLeftAimingDownLeft + 4) and < ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt => ArmCannonDrawing.MovingLeftAimingDownLeft + 2,
            >= (ArmCannonDrawing.FacingLeftMoonwalk + 4) and < ArmCannonDrawing.FacingRightMoonwalk => ArmCannonDrawing.FacingLeftMoonwalk + 2,
            >= (ArmCannonDrawing.FacingRightMoonwalk + 4) and < ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft => ArmCannonDrawing.FacingRightMoonwalk + 2,
            >= (ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 4) and < ArmCannonDrawing.FacingRightMoonwalkAimingUpRight => ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 2,
            >= (ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 4) and < ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft => ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 2,
            >= (ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 4) and < ArmCannonDrawing.FacingRightMoonwalkAimingDownRight => ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 2,
            >= (ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 4) and < ArmCannonDrawing.FacingRightLandingFromNormalJump => ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 2,
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
            >= (ArmCannonDrawing.MovingRightAimingUpRight + 13) and < ArmCannonDrawing.MovingLeftAimingUpLeft => 10,
            >= (ArmCannonDrawing.MovingLeftAimingUpLeft + 13) and < ArmCannonDrawing.MovingRightAimingDownRight => 10,
            >= (ArmCannonDrawing.MovingRightAimingDownRight + 13) and < ArmCannonDrawing.MovingLeftAimingDownLeft => 10,
            >= (ArmCannonDrawing.MovingLeftAimingDownLeft + 13) and < ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt => 10,
            >= (ArmCannonDrawing.FacingLeftMoonwalk + 9) and < ArmCannonDrawing.FacingRightMoonwalk => 6,
            >= (ArmCannonDrawing.FacingRightMoonwalk + 9) and < ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft => 6,
            >= (ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 9) and < ArmCannonDrawing.FacingRightMoonwalkAimingUpRight => 6,
            >= (ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 9) and < ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft => 6,
            >= (ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 9) and < ArmCannonDrawing.FacingRightMoonwalkAimingDownRight => 6,
            >= (ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 9) and < ArmCannonDrawing.FacingRightLandingFromNormalJump => 6,
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
            ArmCannonDrawing.FacingLeft + 3 => ArmCannonDrawing.FacingRight + 3,
            ArmCannonDrawing.FacingLeftAimingDownLeft + 3 => ArmCannonDrawing.FacingRightAimingDownRight + 3,
            ArmCannonDrawing.MovingLeftAimingUpLeft + 3 => ArmCannonDrawing.MovingRightAimingUpRight + 3,
            ArmCannonDrawing.MovingLeftAimingUpLeft + 5 => ArmCannonDrawing.MovingRightAimingUpRight + 5,
            ArmCannonDrawing.MovingLeftAimingUpLeft + 7 => ArmCannonDrawing.MovingRightAimingUpRight + 7,
            ArmCannonDrawing.MovingLeftAimingUpLeft + 9 => ArmCannonDrawing.MovingRightAimingUpRight + 9,
            ArmCannonDrawing.MovingLeftAimingUpLeft + 11 => ArmCannonDrawing.MovingRightAimingUpRight + 11,
            ArmCannonDrawing.MovingLeftAimingDownLeft + 3 => ArmCannonDrawing.MovingRightAimingDownRight + 3,
            ArmCannonDrawing.MovingLeftAimingDownLeft + 5 => ArmCannonDrawing.MovingRightAimingDownRight + 5,
            ArmCannonDrawing.MovingLeftAimingDownLeft + 7 => ArmCannonDrawing.MovingRightAimingDownRight + 7,
            ArmCannonDrawing.MovingLeftAimingDownLeft + 9 => ArmCannonDrawing.MovingRightAimingDownRight + 9,
            ArmCannonDrawing.MovingLeftAimingDownLeft + 11 => ArmCannonDrawing.MovingRightAimingDownRight + 11,
            ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt + 3 => ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt + 3,
            ArmCannonDrawing.FacingLeftNormalJumpAimingDown + 3 => ArmCannonDrawing.FacingRightNormalJumpAimingDown + 3,
            ArmCannonDrawing.FacingLeftNormalJumpMovingForward + 3 => ArmCannonDrawing.FacingRightNormalJumpMovingForward + 3,
            ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft + 3 => ArmCannonDrawing.FacingRightNormalJumpAimingUpRight + 3,
            ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft + 3 => ArmCannonDrawing.FacingRightNormalJumpAimingDownRight + 3,
            ArmCannonDrawing.FacingLeftFallingGunExtended + 3 => ArmCannonDrawing.FacingRightFallingGunExtended + 3,
            ArmCannonDrawing.FacingLeftFallingAimingDown + 3 => ArmCannonDrawing.FacingRightFallingAimingDown + 3,
            ArmCannonDrawing.FacingLeftFallingAimingUpLeft + 3 => ArmCannonDrawing.FacingRightFallingAimingUpRight + 3,
            ArmCannonDrawing.FacingLeftFallingAimingDownLeft + 3 => ArmCannonDrawing.FacingRightFallingAimingDownRight + 3,
            ArmCannonDrawing.FacingLeftCrouching + 3 => ArmCannonDrawing.FacingRightCrouching + 3,
            ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft + 3 => ArmCannonDrawing.FacingRightCrouchingAimingDownRight + 3,
            ArmCannonDrawing.FacingRightMoonwalk + 3 => ArmCannonDrawing.FacingLeftMoonwalk + 3,
            ArmCannonDrawing.FacingRightMoonwalk + 5 => ArmCannonDrawing.FacingLeftMoonwalk + 5,
            ArmCannonDrawing.FacingRightMoonwalk + 7 => ArmCannonDrawing.FacingLeftMoonwalk + 7,
            ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 3 => ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 3,
            ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 5 => ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 5,
            ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 7 => ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 7,
            ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 3 => ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 3,
            ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 5 => ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 5,
            ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 7 => ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 7,
            ArmCannonDrawing.FacingLeftTransitionAimingUp + 3 => ArmCannonDrawing.FacingRightTransitionAimingUp + 3,
            ArmCannonDrawing.FacingLeftAimingUp + 7 => ArmCannonDrawing.FacingRightAimingUp + 7,
            ArmCannonDrawing.FacingLeftNormalJumpAimingUp + 7 => ArmCannonDrawing.FacingRightNormalJumpAimingUp + 7,
            ArmCannonDrawing.FacingLeftCrouchingAimingUp + 7 => ArmCannonDrawing.FacingRightCrouchingAimingUp + 7,
            _ => -1,
        };
        source = pairedYSource >= 0 ? pairedYSource : 0;
        return pairedYSource >= 0;
    }

    /// <summary>$90:C663 DrawArmCannon emits one small OBJ and transfers one32-byte4bpp tile: its horizontal footprint is eight pixels.</summary>
    private const int CoverWidthPixels = 8;

    /// <summary>Mirrors a signed cover X offset around Samus while accounting for its eight-pixel width.</summary>
    internal static byte ReflectCoverX(byte coordinate) => unchecked((byte)(-unchecked((sbyte)coordinate) - CoverWidthPixels));

    /// <summary>Opposite-facing cover origins reflect the eight-pixel footprint around Samus's origin.</summary>
    /// <remarks>Only exact named facing pairs are included. Other coordinates resolve through
    /// the body attachment calculation, including asymmetric downward joins. Each source
    /// precedes its reflected result in the native window.</remarks>
    internal static bool TryStockReflectedXSource(int position, out int source)
    {
        int selected = position switch
        {
            ArmCannonDrawing.FacingLeft + 2 => ArmCannonDrawing.FacingRight + 2,
            ArmCannonDrawing.FacingLeftAimingUp + 4 => ArmCannonDrawing.FacingRightAimingUp + 4,
            ArmCannonDrawing.FacingLeftAimingUp + 6 => ArmCannonDrawing.FacingRightAimingUp + 6,
            ArmCannonDrawing.FacingLeftAimingUpLeft + 2 => ArmCannonDrawing.FacingRightAimingUpRight + 2,
            ArmCannonDrawing.FacingLeftAimingDownLeft + 2 => ArmCannonDrawing.FacingRightAimingDownRight + 2,
            ArmCannonDrawing.MovingLeftGunExtended + 2 => ArmCannonDrawing.MovingRightGunExtended + 2,
            ArmCannonDrawing.MovingLeftAimingUpLeft + 2 => ArmCannonDrawing.MovingRightAimingUpRight + 2,
            ArmCannonDrawing.MovingLeftAimingDownLeft + 2 => ArmCannonDrawing.MovingRightAimingDownRight + 2,
            ArmCannonDrawing.FacingLeftNormalJumpNotMovingGunExt + 2 => ArmCannonDrawing.FacingRightNormalJumpNotMovingGunExt + 2,
            ArmCannonDrawing.FacingLeftNormalJumpAimingUp + 4 => ArmCannonDrawing.FacingRightNormalJumpAimingUp + 4,
            ArmCannonDrawing.FacingLeftNormalJumpAimingUp + 6 => ArmCannonDrawing.FacingRightNormalJumpAimingUp + 6,
            ArmCannonDrawing.FacingLeftNormalJumpMovingForward + 2 => ArmCannonDrawing.FacingRightNormalJumpMovingForward + 2,
            ArmCannonDrawing.FacingLeftNormalJumpAimingUpLeft + 2 => ArmCannonDrawing.FacingRightNormalJumpAimingUpRight + 2,
            ArmCannonDrawing.FacingLeftNormalJumpAimingDownLeft + 2 => ArmCannonDrawing.FacingRightNormalJumpAimingDownRight + 2,
            ArmCannonDrawing.FacingLeftFallingGunExtended + 2 => ArmCannonDrawing.FacingRightFallingGunExtended + 2,
            ArmCannonDrawing.FacingLeftFallingAimingUp + 4 => ArmCannonDrawing.FacingRightFallingAimingUp + 4,
            ArmCannonDrawing.FacingLeftFallingAimingUp + 6 => ArmCannonDrawing.FacingRightFallingAimingUp + 6,
            ArmCannonDrawing.FacingLeftFallingAimingUp + 8 => ArmCannonDrawing.FacingRightFallingAimingUp + 8,
            ArmCannonDrawing.FacingLeftFallingAimingUpLeft + 2 => ArmCannonDrawing.FacingRightFallingAimingUpRight + 2,
            ArmCannonDrawing.FacingLeftFallingAimingDownLeft + 2 => ArmCannonDrawing.FacingRightFallingAimingDownRight + 2,
            ArmCannonDrawing.FacingLeftCrouching + 2 => ArmCannonDrawing.FacingRightCrouching + 2,
            ArmCannonDrawing.FacingLeftCrouchingAimingUpLeft + 2 => ArmCannonDrawing.FacingRightCrouchingAimingUpRight + 2,
            ArmCannonDrawing.FacingLeftCrouchingAimingDownLeft + 2 => ArmCannonDrawing.FacingRightCrouchingAimingDownRight + 2,
            ArmCannonDrawing.FacingLeftCrouchingAimingUp + 4 => ArmCannonDrawing.FacingRightCrouchingAimingUp + 4,
            ArmCannonDrawing.FacingLeftCrouchingAimingUp + 6 => ArmCannonDrawing.FacingRightCrouchingAimingUp + 6,
            ArmCannonDrawing.FacingRightMoonwalk + 2 => ArmCannonDrawing.FacingLeftMoonwalk + 2,
            ArmCannonDrawing.FacingRightMoonwalkAimingUpRight + 2 => ArmCannonDrawing.FacingLeftMoonwalkAimingUpLeft + 2,
            ArmCannonDrawing.FacingRightMoonwalkAimingDownRight + 2 => ArmCannonDrawing.FacingLeftMoonwalkAimingDownLeft + 2,
            ArmCannonDrawing.FacingLeftTransitionAimingUp + 2 => ArmCannonDrawing.FacingRightTransitionAimingUp + 2,
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
    /// <summary>$90:C791..C7A4 assigns OBJ character $1F, palette4 and priority2 to the cannon cover.</summary>
    private const ushort CoverSpriteIdentity = 31 | (4 << 9) | (2 << 12);
    /// <summary>$90:C791..C7A4 uses bit14 to reflect the selected cover artwork horizontally.</summary>
    private const ushort HorizontalReflection = 1 << 14;
    /// <summary>$90:C791..C7A4 uses bit15 to reflect the vertical source for downward aim.</summary>
    private const ushort VerticalReflection = 1 << 15;

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
        return (int)orientation * (FramesPerDirection - 1) + frame - 1;
    }

    /// <summary>$90:C791..C7A4 reflects the selected vertical/horizontal/diagonal artwork according to native aiming direction.</summary>
    internal static ushort StockSpriteAttributes(SamusProjectileDirection direction)
    {
        bool horizontal = direction is SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingLeft
            or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft;
        bool vertical = direction is SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft;
        return (ushort)(CoverSpriteIdentity | (horizontal ? HorizontalReflection : 0) | (vertical ? VerticalReflection : 0));
    }
}

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Where each arm-cannon drawing descriptor selected by Samus pose starts in the drawing window.</summary>
internal static class ArmCannonDescriptorDefinitions
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

namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A8 code pointers consumed by translated Work Robot dispatchers.</summary>
internal static class WorkRobotInstructionCodes
{
    /// <summary><c>Instruction_Robot_FacingLeft_MoveForward_HandleWallOrFall</c> at $A8:CD09.</summary>
    public const ushort Instruction_Robot_FacingLeft_MoveForward_HandleWallOrFall = 0xcd09;

    /// <summary><c>Instruction_Robot_FacingLeft_MoveForward_HandleHittingWall</c> at $A8:CDA4.</summary>
    public const ushort Instruction_Robot_FacingLeft_MoveForward_HandleHittingWall = 0xcda4;

    /// <summary><c>Instruction_Robot_FacingLeft_MoveBackward_HandleWallOrFall</c> at $A8:CDEA.</summary>
    public const ushort Instruction_Robot_FacingLeft_MoveBackward_HandleWallOrFall = 0xcdea;

    /// <summary><c>Instruction_Robot_FacingLeft_MoveBackward_HandleHittingWall</c> at $A8:CE85.</summary>
    public const ushort Instruction_Robot_FacingLeft_MoveBackward_HandleHittingWall = 0xce85;

    /// <summary><c>Instruction_Robot_SetInstListTo_FacingRight_WalkingForwards</c> at $A8:CECB.</summary>
    public const ushort Instruction_Robot_SetInstListTo_FacingRight_WalkingForwards = 0xcecb;

    /// <summary><c>Instruction_Robot_FacingRight_MoveForward_HandleWallOrFall</c> at $A8:CECF.</summary>
    public const ushort Instruction_Robot_FacingRight_MoveForward_HandleWallOrFall = 0xcecf;

    /// <summary><c>Instruction_Robot_FacingRight_MoveForward_HandleHittingWall</c> at $A8:CF6A.</summary>
    public const ushort Instruction_Robot_FacingRight_MoveForward_HandleHittingWall = 0xcf6a;

    /// <summary><c>Instruction_Robot_FacingRight_MoveBackward_HandleWallOrFall</c> at $A8:CFB0.</summary>
    public const ushort Instruction_Robot_FacingRight_MoveBackward_HandleWallOrFall = 0xcfb0;

    /// <summary><c>Instruction_Robot_FacingRight_MoveBackward_HandleHittingWall</c> at $A8:D04B.</summary>
    public const ushort Instruction_Robot_FacingRight_MoveBackward_HandleHittingWall = 0xd04b;

    /// <summary><c>Instruction_Robot_PlaySFXIfOnScreen</c> at $A8:D091.</summary>
    public const ushort Instruction_Robot_PlaySFXIfOnScreen = 0xd091;

    /// <summary><c>Instruction_Robot_Goto_FacingLeft_WalkingForwards</c> at $A8:D0C2.</summary>
    public const ushort Instruction_Robot_Goto_FacingLeft_WalkingForwards = 0xd0c2;

    /// <summary><c>Instruction_Robot_TryShootingLaserUpRight</c> at $A8:D0C6.</summary>
    public const ushort Instruction_Robot_TryShootingLaserUpRight = 0xd0c6;

    /// <summary><c>Instruction_Robot_TryShootingLaserUpLeft</c> at $A8:D0D2.</summary>
    public const ushort Instruction_Robot_TryShootingLaserUpLeft = 0xd0d2;

    /// <summary><c>Instruction_Robot_TryShootingLaserRight</c> at $A8:D100.</summary>
    public const ushort Instruction_Robot_TryShootingLaserRight = 0xd100;

    /// <summary><c>Instruction_Robot_TryShootingLaserLeft</c> at $A8:D107.</summary>
    public const ushort Instruction_Robot_TryShootingLaserLeft = 0xd107;

    /// <summary><c>Instruction_Robot_TryShootingLaserDownRight</c> at $A8:D131.</summary>
    public const ushort Instruction_Robot_TryShootingLaserDownRight = 0xd131;

    /// <summary><c>Instruction_Robot_TryShootingLaserDownLeft</c> at $A8:D13D.</summary>
    public const ushort Instruction_Robot_TryShootingLaserDownLeft = 0xd13d;

    /// <summary><c>Instruction_Robot_DecrementLaserCooldown</c> at $A8:D16B.</summary>
    public const ushort Instruction_Robot_DecrementLaserCooldown = 0xd16b;

}

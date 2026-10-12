using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for all wall Space Pirate body programs.
/// Interleaved extended-spritemap pointers remain live cartridge presentation data.
/// </summary>
internal abstract class WallSpacePirateInstructionProgramDefinitions
{
    /// <summary><c>InstList_PirateWall_FireLaser_WallJumpLeft</c> at $B2:ECC0.</summary>
    internal const ushort FireAndJumpLeft = 0xecc0;
    /// <summary><c>InstList_PirateWall_LandedOnLeftWall</c> at $B2:ECE4.</summary>
    internal const ushort LandedOnLeftWall = 0xece4;
    /// <summary><c>InstList_PirateWall_MovingUpLeftWall_0</c> at $B2:ECEC.</summary>
    internal const ushort MovingUpLeftWall = 0xecec;
    /// <summary><c>InstList_PirateWall_MovingDownLeftWall_0</c> at $B2:ED36.</summary>
    internal const ushort MovingDownLeftWall = 0xed36;
    /// <summary><c>InstList_PirateWall_FireLaser_WallJumpRight</c> at $B2:ED80.</summary>
    internal const ushort FireAndJumpRight = 0xed80;
    /// <summary><c>InstList_PirateWall_LandingOnRightWall</c> at $B2:EDA4.</summary>
    internal const ushort LandedOnRightWall = 0xeda4;
    /// <summary><c>InstList_PirateWall_MovingDownRightWall_0</c> at $B2:EDAC.</summary>
    internal const ushort MovingDownRightWall = 0xedac;
    /// <summary><c>InstList_PirateWall_MovingUpRightWall_0</c> at $B2:EDF6.</summary>
    internal const ushort MovingUpRightWall = 0xedf6;

    /// <summary><c>InstList_PirateWall_MovingUpLeftWall_1</c> at $B2:ECF4.</summary>
    private const ushort PirateWallMovingUpLeftWall1 = 0xecf4;
    /// <summary><c>InstList_PirateWall_MovingDownLeftWall_1</c> at $B2:ED3E.</summary>
    private const ushort PirateWallMovingDownLeftWall1 = 0xed3e;
    /// <summary><c>InstList_PirateWall_MovingDownRightWall_1</c> at $B2:EDB4.</summary>
    private const ushort PirateWallMovingDownRightWall1 = 0xedb4;
    /// <summary><c>InstList_PirateWall_MovingUpRightWall_1</c> at $B2:EDFE.</summary>
    private const ushort PirateWallMovingUpRightWall1 = 0xedfe;
    /// <summary><c>Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Left</c> at $B2:EE40.</summary>
    private const ushort InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft = 0xee40;
    /// <summary><c>Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Right</c> at $B2:EE72.</summary>
    private const ushort InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight = 0xee72;
    /// <summary><c>Instruction_PirateWall_RandomlyChooseADirection_LeftWall</c> at $B2:EEA4.</summary>
    private const ushort PirateWallRandomlyChooseADirectionLeftWall = 0xeea4;
    /// <summary><c>Instruction_PirateWall_RandomlyChooseADirection_RightWall</c> at $B2:EEBC.</summary>
    private const ushort PirateWallRandomlyChooseADirectionRightWall = 0xeebc;
    /// <summary><c>Instruction_PirateWall_PrepareWallJumpToRight</c> at $B2:EED4.</summary>
    private const ushort PirateWallPrepareWallJumpToRight = 0xeed4;
    /// <summary><c>Instruction_PirateWall_PrepareWallJumpToLeft</c> at $B2:EEFD.</summary>
    private const ushort PirateWallPrepareWallJumpToLeft = 0xeefd;
    /// <summary><c>Instruction_PirateWall_FireLaserLeft</c> at $B2:EF2A.</summary>
    private const ushort PirateWallFireLaserLeft = 0xef2a;
    /// <summary><c>Instruction_PirateWall_FireLaserRight</c> at $B2:EF5D.</summary>
    private const ushort PirateWallFireLaserRight = 0xef5d;
    /// <summary><c>Instruction_PirateWall_FunctionInY</c> at $B2:EF83.</summary>
    private const ushort PirateWallFunctionInY = 0xef83;
    /// <summary><c>Instruction_PirateWall_QueueSpacePirateAttackSFX</c> at $B2:EF93.</summary>
    private const ushort PirateWallQueueSpacePirateAttackSFX = 0xef93;
    /// <summary><c>Function_PirateWall_ClimbingLeftWall</c> at $B2:F034.</summary>
    private const ushort PirateWallClimbingLeftWallFunction = 0xf034;
    /// <summary><c>RTS_B2F04F</c> at $B2:F04F.</summary>
    private const ushort EmptyRoutineF04F = 0xf04f;
    /// <summary><c>Function_PirateWall_WallJumpingRight</c> at $B2:F050.</summary>
    private const ushort PirateWallWallJumpingRightFunction = 0xf050;
    /// <summary><c>Function_PirateWall_ClimbingRightWall</c> at $B2:F0C8.</summary>
    private const ushort PirateWallClimbingRightWallFunction = 0xf0c8;
    /// <summary><c>RTS_B2F0E3</c> at $B2:F0E3.</summary>
    private const ushort EmptyRoutineF0E3 = 0xf0e3;
    /// <summary><c>Function_PirateWall_WallJumpingLeft</c> at $B2:F0E4.</summary>
    private const ushort PirateWallWallJumpingLeftFunction = 0xf0e4;

    /// <summary>Native program bank $B2.</summary>
    internal const byte Bank = 0xb2;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xecc0),
        Entry(FireAndJumpLeft),
        Op(PirateWallFunctionInY, EmptyRoutineF0E3),
        Frame(9),
        Frame(15),
        Op(PirateWallFireLaserLeft),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0020),
        Op(PirateWallPrepareWallJumpToLeft),
        Op(PirateWallFunctionInY, PirateWallWallJumpingLeftFunction),
        Op(PirateWallQueueSpacePirateAttackSFX),
        Frame(10),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Entry(LandedOnLeftWall),
        Op(PirateWallFunctionInY, PirateWallClimbingLeftWallFunction),
        Frame(10),
        Entry(MovingUpLeftWall),
        Op(PirateWallFunctionInY, PirateWallClimbingLeftWallFunction),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Frame(10),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0xfffd),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0xfffd),
        Frame(5),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0xfffd),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0xfffd),
        Frame(10),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0xfffd),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0xfffd),
        Frame(5),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0xfffd),
        Frame(8),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, PirateWallMovingUpLeftWall1),
        Op(PirateWallRandomlyChooseADirectionLeftWall),
        Entry(MovingDownLeftWall),
        Op(PirateWallFunctionInY, PirateWallClimbingLeftWallFunction),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Frame(10),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0x0003),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0x0003),
        Frame(5),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0x0003),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0x0003),
        Frame(10),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0x0003),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0x0003),
        Frame(5),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionLeft, 0x0003),
        Frame(8),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, PirateWallMovingDownLeftWall1),
        Op(PirateWallRandomlyChooseADirectionLeftWall),
        Entry(FireAndJumpRight),
        Op(PirateWallFunctionInY, EmptyRoutineF04F),
        Frame(9),
        Frame(1),
        Op(PirateWallFireLaserRight),
        Op((ushort)CommonEnemyInstruction.WaitFrames, 0x0020),
        Op(PirateWallPrepareWallJumpToRight),
        Op(PirateWallFunctionInY, PirateWallWallJumpingRightFunction),
        Op(PirateWallQueueSpacePirateAttackSFX),
        Frame(10),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Entry(LandedOnRightWall),
        Op(PirateWallFunctionInY, PirateWallClimbingRightWallFunction),
        Frame(10),
        Entry(MovingDownRightWall),
        Op(PirateWallFunctionInY, PirateWallClimbingRightWallFunction),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Frame(10),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0x0003),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0x0003),
        Frame(5),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0x0003),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0x0003),
        Frame(10),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0x0003),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0x0003),
        Frame(5),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0x0003),
        Frame(8),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, PirateWallMovingDownRightWall1),
        Op(PirateWallRandomlyChooseADirectionRightWall),
        Entry(MovingUpRightWall),
        Op(PirateWallFunctionInY, PirateWallClimbingRightWallFunction),
        Op((ushort)CommonEnemyInstruction.SetTimer, 0x0004),
        Frame(10),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0xfffd),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0xfffd),
        Frame(5),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0xfffd),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0xfffd),
        Frame(10),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0xfffd),
        Frame(8),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0xfffd),
        Frame(5),
        Op(InstPirateWallMoveYPixelsDownChangeDirOnCollisionRight, 0xfffd),
        Frame(8),
        Op((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate, PirateWallMovingUpRightWall1),
        Op(PirateWallRandomlyChooseADirectionRightWall));
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Wall Space Pirate instruction mechanics pointer $B2:{address:X4} is not compiled.");
}

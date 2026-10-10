using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned bank-$AA control words for the two shared Bomb/Golden Torizo
/// left-facing backward-jump lists at $AA:BC96-BD0D. Eight interleaved frame
/// pointers remain presentation selectors, independent of the instruction
/// callbacks, durations, and branch destinations compiled here.
/// </summary>
internal abstract class TorizoJumpBackLeftInstructionProgramDefinitions
{
    internal const ushort Start = 0xbc96;

    /// <summary><c>InstList_Torizo_FacingLeft_Walking_RightLegMoving</c> at $AA:B96C.</summary>
    private const ushort FacingLeftWalkingRightLegMoving = 0xb96c;
    /// <summary><c>InstList_Torizo_FacingLeft_Walking_LeftLegMoving</c> at $AA:B9B6.</summary>
    private const ushort FacingLeftWalkingLeftLegMoving = 0xb9b6;
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_0</c> at $AA:BA04.</summary>
    private const ushort FacingLeftSpewingChozoOrbsRightFootFwd0 = 0xba04;
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_0</c> at $AA:BA46.</summary>
    private const ushort FacingLeftSpewingChozoOrbsLeftFootFwd0 = 0xba46;
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_0</c> at $AA:BA88.</summary>
    private const ushort FacingLeftSonicBoomsRightFootForward0 = 0xba88;
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_0</c> at $AA:BAF2.</summary>
    private const ushort FacingLeftSonicBoomsLeftFootForward0 = 0xbaf2;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_1</c> at $AA:BCA6.</summary>
    private const ushort FacingLeftJumpingBackwardLandLeftFootFwd1 = 0xbca6;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_2</c> at $AA:BCAE.</summary>
    private const ushort FacingLeftJumpingBackwardLandLeftFootFwd2 = 0xbcae;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_3</c> at $AA:BCB6.</summary>
    private const ushort FacingLeftJumpingBackwardLandLeftFootFwd3 = 0xbcb6;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_4</c> at $AA:BCBE.</summary>
    private const ushort FacingLeftJumpingBackwardLandLeftFootFwd4 = 0xbcbe;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_1</c> at $AA:BCE2.</summary>
    private const ushort FacingLeftJumpingBackwardRightFootFwd1 = 0xbce2;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_2</c> at $AA:BCEA.</summary>
    private const ushort FacingLeftJumpingBackwardRightFootFwd2 = 0xbcea;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_3</c> at $AA:BCF2.</summary>
    private const ushort FacingLeftJumpingBackwardRightFootFwd3 = 0xbcf2;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_4</c> at $AA:BCFA.</summary>
    private const ushort FacingLeftJumpingBackwardRightFootFwd4 = 0xbcfa;
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_RightLegMoving</c> at $AA:BD18.</summary>
    private const ushort FacingLeftFacelessWalkingRightLegMoving = 0xbd18;
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving</c> at $AA:BD52.</summary>
    private const ushort FacingLeftFacelessWalkingLeftLegMoving = 0xbd52;
    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c> at $AA:C82C.</summary>
    private const ushort MovementJumpingFallingFunction = 0xc82c;
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingLeft_LeftFootFwd</c> at $AA:CDAF.</summary>
    private const ushort GTLandedFromBackwardsJumpFacingLeftLeftFootFwd = 0xcdaf;
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingLeft_RightFootFwd</c> at $AA:CDB9.</summary>
    private const ushort GTLandedFromBackwardsJumpFacingLeftRightFootFwd = 0xcdb9;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xbc96),
        Entry(Start),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, MovementJumpingFallingFunction),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, FacingLeftJumpingBackwardLandLeftFootFwd2),
        Frame(5),
        Frame(5),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfRising, FacingLeftJumpingBackwardLandLeftFootFwd1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, MovementJumpingFallingFunction),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, FacingLeftJumpingBackwardLandLeftFootFwd4),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, FacingLeftJumpingBackwardLandLeftFootFwd3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, FacingLeftFacelessWalkingRightLegMoving, GTLandedFromBackwardsJumpFacingLeftLeftFootFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack, FacingLeftSpewingChozoOrbsLeftFootFwd0, FacingLeftSonicBoomsLeftFootForward0),
        Op((ushort)CommonEnemyInstruction.Goto, FacingLeftWalkingRightLegMoving),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, MovementJumpingFallingFunction),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, FacingLeftJumpingBackwardRightFootFwd2),
        Frame(5),
        Frame(5),
        Frame(1),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfRising, FacingLeftJumpingBackwardRightFootFwd1),
        Op((ushort)TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY, MovementJumpingFallingFunction),
        Op((ushort)TorizoInstruction.Instruction_Torizo_LinkInstructionInY, FacingLeftJumpingBackwardRightFootFwd4),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, FacingLeftJumpingBackwardRightFootFwd3),
        Op((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, FacingLeftFacelessWalkingLeftLegMoving, GTLandedFromBackwardsJumpFacingLeftRightFootFwd),
        Op((ushort)TorizoInstruction.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack, FacingLeftSpewingChozoOrbsRightFootFwd0, FacingLeftSonicBoomsRightFootForward0),
        Op((ushort)CommonEnemyInstruction.Goto, FacingLeftWalkingLeftLegMoving));
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);
}

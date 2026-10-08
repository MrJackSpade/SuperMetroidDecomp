using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned bank-$AA control words for the two shared Bomb/Golden Torizo
/// right-facing backward-jump lists at $AA:C110-C187. Their eight interleaved
/// selectors use three editable visual frames; link targets, movement callbacks,
/// durations and branch operands remain compiled cartridge mechanics.
/// </summary>
internal abstract class TorizoJumpBackInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    internal const ushort Start = 0xc110;

    /// <summary><c>InstList_Torizo_FacingRight_Walking_LeftLegMoving</c> at $AA:BDE2.</summary>
    private const ushort FacingRightWalkingLeftLegMoving = 0xbde2;
    /// <summary><c>InstList_Torizo_FacingRight_Walking_RightLegMoving</c> at $AA:BE30.</summary>
    private const ushort FacingRightWalkingRightLegMoving = 0xbe30;
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_0</c> at $AA:BE7E.</summary>
    private const ushort FacingRightSpewingChozoOrbsLeftFootFwd0 = 0xbe7e;
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_0</c> at $AA:BEC0.</summary>
    private const ushort FacingRightSpewingChozoOrbsRightFootFwd0 = 0xbec0;
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_0</c> at $AA:BF02.</summary>
    private const ushort FacingRightSonicBoomsLeftFootForward0 = 0xbf02;
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_RightFootForward_0</c> at $AA:BF6C.</summary>
    private const ushort FacingRightSonicBoomsRightFootForward0 = 0xbf6c;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_1</c> at $AA:C120.</summary>
    private const ushort FacingRightJumpBackwardLandRightFootFwd1 = 0xc120;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_2</c> at $AA:C128.</summary>
    private const ushort FacingRightJumpBackwardLandRightFootFwd2 = 0xc128;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_3</c> at $AA:C130.</summary>
    private const ushort FacingRightJumpBackwardLandRightFootFwd3 = 0xc130;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_4</c> at $AA:C138.</summary>
    private const ushort FacingRightJumpBackwardLandRightFootFwd4 = 0xc138;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_1</c> at $AA:C15C.</summary>
    private const ushort FacingRightJumpBackwardsLandLeftFootFwd1 = 0xc15c;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_2</c> at $AA:C164.</summary>
    private const ushort FacingRightJumpBackwardsLandLeftFootFwd2 = 0xc164;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_3</c> at $AA:C16C.</summary>
    private const ushort FacingRightJumpBackwardsLandLeftFootFwd3 = 0xc16c;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_4</c> at $AA:C174.</summary>
    private const ushort FacingRightJumpBackwardsLandLeftFootFwd4 = 0xc174;
    /// <summary><c>InstList_Torizo_FacingRight_Faceless_Walking_LeftLegMoving</c> at $AA:C192.</summary>
    private const ushort FacingRightFacelessWalkingLeftLegMoving = 0xc192;
    /// <summary><c>InstList_Torizo_FacingRight_Faceless_Walking_RightLegMoving</c> at $AA:C1CC.</summary>
    private const ushort FacingRightFacelessWalkingRightLegMoving = 0xc1cc;
    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c> at $AA:C82C.</summary>
    private const ushort MovementJumpingFallingFunction = 0xc82c;
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingRight_RightFootFwd</c> at $AA:CDC3.</summary>
    private const ushort GTLandedFromBackwardsJumpFacingRightRightFootFwd = 0xcdc3;
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingRight_LeftFootFwd</c> at $AA:CDCD.</summary>
    private const ushort GTLandedFromBackwardsJumpFacingRightLeftFootFwd = 0xcdcd;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xc110),
        Entry(Start),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, MovementJumpingFallingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY, FacingRightJumpBackwardLandRightFootFwd2),
        Frame(5),
        Frame(5),
        Frame(1),
        Op(TorizoInstructionCodes.Instruction_Torizo_GotoY_IfRising, FacingRightJumpBackwardLandRightFootFwd1),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, MovementJumpingFallingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY, FacingRightJumpBackwardLandRightFootFwd4),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, FacingRightJumpBackwardLandRightFootFwd3),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op(TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op(TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, FacingRightFacelessWalkingLeftLegMoving, GTLandedFromBackwardsJumpFacingRightRightFootFwd),
        Op(TorizoInstructionCodes.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack, FacingRightSpewingChozoOrbsRightFootFwd0, FacingRightSonicBoomsRightFootForward0),
        Op(CommonEnemyInstructionCodes.Goto, FacingRightWalkingLeftLegMoving),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, MovementJumpingFallingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY, FacingRightJumpBackwardsLandLeftFootFwd2),
        Frame(5),
        Frame(5),
        Frame(1),
        Op(TorizoInstructionCodes.Instruction_Torizo_GotoY_IfRising, FacingRightJumpBackwardsLandLeftFootFwd1),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, MovementJumpingFallingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY, FacingRightJumpBackwardsLandLeftFootFwd4),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, FacingRightJumpBackwardsLandLeftFootFwd3),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op(TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op(TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, FacingRightFacelessWalkingRightLegMoving, GTLandedFromBackwardsJumpFacingRightLeftFootFwd),
        Op(TorizoInstructionCodes.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack, FacingRightSpewingChozoOrbsLeftFootFwd0, FacingRightSonicBoomsLeftFootForward0),
        Op(CommonEnemyInstructionCodes.Goto, FacingRightWalkingRightLegMoving));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}

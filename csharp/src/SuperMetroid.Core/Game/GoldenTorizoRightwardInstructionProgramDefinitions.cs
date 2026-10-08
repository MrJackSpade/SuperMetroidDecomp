using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned control words for Golden Torizo's two turning-right and two
/// linked walking-right lists at $AA:D2AD-D368. The twelve interleaved frame
/// selectors identify separately editable presentation; timing, movement,
/// callbacks and branches remain fixed cartridge mechanics.
/// </summary>
internal abstract class GoldenTorizoRightwardInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    internal const ushort Start = GoldenTorizoCombatInstructionPointers.DodgeTurningRight;
    internal const ushort End = 0xd369;

    /// <summary><c>InstList_Torizo_FacingRight_JumpingForwards_0</c> at $AA:C0DA.</summary>
    private const ushort TorizoFacingRightJumpingForwards0 = 0xc0da;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0</c> at $AA:C110.</summary>
    private const ushort TorizoFacingRightJumpBackwardLandRightFootFwd0 = 0xc110;
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0</c> at $AA:C14C.</summary>
    private const ushort TorizoFacingRightJumpBackwardsLandLeftFootFwd0 = 0xc14c;
    /// <summary><c>Function_Torizo_SimpleMovement</c> at $AA:C6BF.</summary>
    private const ushort TorizoSimpleMovementFunction = 0xc6bf;
    /// <summary><c>InstList_GoldenTorizo_SitDownAttack_FacingRight</c> at $AA:CFC5.</summary>
    private const ushort TorizoSitDownAttackFacingRight = 0xcfc5;
    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0</c> at $AA:D031.</summary>
    private const ushort TorizoReleaseGoldenTorizoEggs0 = 0xd031;
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_0</c> at $AA:D10D.</summary>
    private const ushort TorizoEyeBeamAttack0 = 0xd10d;
    /// <summary><c>InstList_Torizo_Stunned_0</c> at $AA:D193.</summary>
    private const ushort TorizoStunned0 = 0xd193;
    /// <summary><c>InstList_GoldenTorizo_WalkingRight_LeftLegMoving</c> at $AA:D2C9.</summary>
    private const ushort TorizoWalkingRightLeftLegMoving = 0xd2c9;
    /// <summary><c>Function_GoldenTorizo_NormalMovement</c> at $AA:D5E6.</summary>
    private const ushort TorizoNormalMovementFunction = 0xd5e6;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort TorizoMovementWalkingFunction = 0xd5f1;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xd2ad),
        Entry(GoldenTorizoCombatInstructionPointers.DodgeTurningRight),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, TorizoSimpleMovementFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(24),
        Op(TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        Op(CommonEnemyInstructionCodes.Goto, TorizoWalkingRightLeftLegMoving),
        Entry(GoldenTorizoCombatInstructionPointers.TurningRight),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, TorizoSimpleMovementFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(8),
        Entry(GoldenTorizoCombatInstructionPointers.WalkingRightLeftLeg),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetSteppedRightWithRightFootState),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, TorizoNormalMovementFunction),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementWalkingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(8),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel, TorizoFacingRightJumpingForwards0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, TorizoFacingRightJumpBackwardLandRightFootFwd0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31, TorizoStunned0),
        Op(TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo, TorizoEyeBeamAttack0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0016),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0018),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789, TorizoReleaseGoldenTorizoEggs0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x001a),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, TorizoFacingRightJumpBackwardLandRightFootFwd0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x001c),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x001e),
        Entry(GoldenTorizoCombatInstructionPointers.WalkingRightRightLeg),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetSteppedRightWithLeftFootState),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, TorizoNormalMovementFunction),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementWalkingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(8),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo, TorizoSitDownAttackFacingRight),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel, TorizoFacingRightJumpingForwards0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, TorizoFacingRightJumpBackwardsLandLeftFootFwd0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31, TorizoStunned0),
        Op(TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo, TorizoEyeBeamAttack0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0020),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0022),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789, TorizoReleaseGoldenTorizoEggs0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, TorizoFacingRightJumpBackwardsLandLeftFootFwd0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0024),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0026),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0014),
        Op(CommonEnemyInstructionCodes.Goto, TorizoWalkingRightLeftLegMoving));

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

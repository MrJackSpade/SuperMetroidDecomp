using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned control words for Golden Torizo's linked walking-left lists at
/// $AA:D20D-D2AC. Durations, movement/attack callbacks and branch targets are
/// fixed cartridge mechanics; the ten interleaved extended-frame selectors
/// identify separately editable visual compositions.
/// </summary>
internal abstract class GoldenTorizoWalkingInstructionProgramDefinitions
{

    /// <summary><c>InstList_Torizo_FacingLeft_JumpingForwards_0</c> at $AA:BC60.</summary>
    private const ushort TorizoFacingLeftJumpingForwards0 = 0xbc60;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0</c> at $AA:BC96.</summary>
    private const ushort TorizoFacingLeftJumpingBackwardLandLeftFootFwd0 = 0xbc96;
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0</c> at $AA:BCD2.</summary>
    private const ushort TorizoFacingLeftJumpingBackwardRightFootFwd0 = 0xbcd2;
    /// <summary><c>InstList_GoldenTorizo_SitDownAttack_FacingLeft</c> at $AA:CF59.</summary>
    private const ushort TorizoSitDownAttackFacingLeft = 0xcf59;
    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0</c> at $AA:D031.</summary>
    private const ushort TorizoReleaseGoldenTorizoEggs0 = 0xd031;
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_0</c> at $AA:D10D.</summary>
    private const ushort TorizoEyeBeamAttack0 = 0xd10d;
    /// <summary><c>InstList_Torizo_Stunned_0</c> at $AA:D193.</summary>
    private const ushort TorizoStunned0 = 0xd193;
    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_RightLegMoving</c> at $AA:D20D.</summary>
    private const ushort TorizoWalkingLeftRightLegMoving = 0xd20d;
    /// <summary><c>Function_GoldenTorizo_NormalMovement</c> at $AA:D5E6.</summary>
    private const ushort TorizoNormalMovementFunction = 0xd5e6;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort TorizoMovementWalkingFunction = 0xd5f1;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>Describes the walking instruction lists and marks their editable extended-frame selector words.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xd20d),
        Entry(GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithLeftFootState),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, TorizoNormalMovementFunction),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementWalkingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(8),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel, TorizoFacingLeftJumpingForwards0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, TorizoFacingLeftJumpingBackwardLandLeftFootFwd0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31, TorizoStunned0),
        Op(TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo, TorizoEyeBeamAttack0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0002),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0004),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789, TorizoReleaseGoldenTorizoEggs0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, TorizoFacingLeftJumpingBackwardLandLeftFootFwd0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0006),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0008),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x000a),
        Entry(GoldenTorizoCombatInstructionPointers.WalkingLeftLeftLeg),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, TorizoNormalMovementFunction),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementWalkingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Frame(8),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo, TorizoSitDownAttackFacingLeft),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel, TorizoFacingLeftJumpingForwards0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, TorizoFacingLeftJumpingBackwardRightFootFwd0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31, TorizoStunned0),
        Op(TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo, TorizoEyeBeamAttack0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x000c),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x000e),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789, TorizoReleaseGoldenTorizoEggs0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels, TorizoFacingLeftJumpingBackwardRightFootFwd0),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0010),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0012),
        Frame(4),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY, 0x0000),
        Op(CommonEnemyInstructionCodes.Goto, TorizoWalkingLeftRightLegMoving));
    /// <summary>Gets the number of extended-frame selector words embedded in the walking lists.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    /// <summary>Gets the native word address of a walking-list extended-frame selector.</summary>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);
}

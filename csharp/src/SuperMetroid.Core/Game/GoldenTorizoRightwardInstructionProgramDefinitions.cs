namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoRightwardMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// Engine-owned control words for Golden Torizo's two turning-right and two
/// linked walking-right lists at $AA:D2AD-D368. The twelve interleaved frame
/// selectors identify separately editable presentation; timing, movement,
/// callbacks and branches remain fixed cartridge mechanics.
/// </summary>
internal static class GoldenTorizoRightwardInstructionProgramDefinitions
{
    internal const ushort Start = GoldenTorizoCombatInstructionPointers.DodgeTurningRight;
    internal const ushort End = 0xd369;

    private static readonly GoldenTorizoRightwardMechanicsWord[] Words =
    [
        new(0xd2ad, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd2af, 0xc6bf),
        new(0xd2b1, TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        new(0xd2b3, TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag),
        new(0xd2b5, 0x0018),
        new(0xd2b9, TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        new(0xd2bb, CommonEnemyInstructionCodes.Goto),
        new(0xd2bd, GoldenTorizoCombatInstructionPointers.WalkingRightLeftLeg),
        new(0xd2bf, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd2c1, 0xc6bf),
        new(0xd2c3, TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag),
        new(0xd2c5, 0x0008),
        new(0xd2c9, TorizoInstructionCodes.Instruction_Torizo_SetSteppedRightWithRightFootState),
        new(0xd2cb, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd2cd, 0xd5e6),
        new(0xd2cf, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xd2d1, 0xd5f1),
        new(0xd2d3, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xd2d5, 0x0008),
        new(0xd2d9, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel),
        new(0xd2db, 0xc0da),
        new(0xd2dd, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels),
        new(0xd2df, 0xc110),
        new(0xd2e1, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31),
        new(0xd2e3, 0xd193),
        new(0xd2e5, TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo),
        new(0xd2e7, 0xd10d),
        new(0xd2e9, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd2eb, 0x0016),
        new(0xd2ed, 0x0004),
        new(0xd2f1, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd2f3, 0x0018),
        new(0xd2f5, 0x0004),
        new(0xd2f9, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789),
        new(0xd2fb, 0xd031),
        new(0xd2fd, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd2ff, 0x001a),
        new(0xd301, 0x0004),
        new(0xd305, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels),
        new(0xd307, 0xc110),
        new(0xd309, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd30b, 0x001c),
        new(0xd30d, 0x0004),
        new(0xd311, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd313, 0x001e),
        new(0xd315, TorizoInstructionCodes.Instruction_Torizo_SetSteppedRightWithLeftFootState),
        new(0xd317, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd319, 0xd5e6),
        new(0xd31b, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xd31d, 0xd5f1),
        new(0xd31f, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xd321, 0x0008),
        new(0xd325, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo),
        new(0xd327, 0xcfc5),
        new(0xd329, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel),
        new(0xd32b, 0xc0da),
        new(0xd32d, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels),
        new(0xd32f, 0xc14c),
        new(0xd331, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31),
        new(0xd333, 0xd193),
        new(0xd335, TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo),
        new(0xd337, 0xd10d),
        new(0xd339, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd33b, 0x0020),
        new(0xd33d, 0x0004),
        new(0xd341, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd343, 0x0022),
        new(0xd345, 0x0004),
        new(0xd349, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789),
        new(0xd34b, 0xd031),
        new(0xd34d, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels),
        new(0xd34f, 0xc14c),
        new(0xd351, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd353, 0x0024),
        new(0xd355, 0x0004),
        new(0xd359, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd35b, 0x0026),
        new(0xd35d, 0x0004),
        new(0xd361, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd363, 0x0014),
        new(0xd365, CommonEnemyInstructionCodes.Goto),
        new(0xd367, GoldenTorizoCombatInstructionPointers.WalkingRightLeftLeg),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xd2b7, 0xd2c7, 0xd2d7, 0xd2ef, 0xd2f7, 0xd303,
        0xd30f, 0xd323, 0xd33f, 0xd347, 0xd357, 0xd35f,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoRightwardMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoRightwardMechanicsWord word in Words)
        {
            if (word.Address != address) continue;
            value = word.Value;
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (GoldenTorizoRightwardMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}

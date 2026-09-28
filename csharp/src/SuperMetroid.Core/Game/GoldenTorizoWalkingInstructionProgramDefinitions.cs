namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoWalkingMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// Engine-owned control words for Golden Torizo's linked walking-left lists at
/// $AA:D20D-D2AC. Durations, movement/attack callbacks and branch targets are
/// fixed cartridge mechanics; the ten interleaved extended-frame selectors
/// identify separately editable visual compositions.
/// </summary>
internal static class GoldenTorizoWalkingInstructionProgramDefinitions
{
    internal const ushort Start = GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg;
    internal const ushort End = 0xd2ad;

    private static readonly GoldenTorizoWalkingMechanicsWord[] Words =
    [
        new(0xd20d, TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithLeftFootState),
        new(0xd20f, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd211, 0xd5e6),
        new(0xd213, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xd215, 0xd5f1),
        new(0xd217, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xd219, 0x0008),
        new(0xd21d, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel),
        new(0xd21f, 0xbc60),
        new(0xd221, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels),
        new(0xd223, 0xbc96),
        new(0xd225, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31),
        new(0xd227, 0xd193),
        new(0xd229, TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo),
        new(0xd22b, 0xd10d),
        new(0xd22d, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd22f, 0x0002),
        new(0xd231, 0x0004),
        new(0xd235, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd237, 0x0004),
        new(0xd239, 0x0004),
        new(0xd23d, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789),
        new(0xd23f, 0xd031),
        new(0xd241, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels),
        new(0xd243, 0xbc96),
        new(0xd245, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd247, 0x0006),
        new(0xd249, 0x0004),
        new(0xd24d, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd24f, 0x0008),
        new(0xd251, 0x0004),
        new(0xd255, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd257, 0x000a),
        new(0xd259, TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        new(0xd25b, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd25d, 0xd5e6),
        new(0xd25f, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xd261, 0xd5f1),
        new(0xd263, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xd265, 0x0008),
        new(0xd269, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo),
        new(0xd26b, 0xcf59),
        new(0xd26d, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel),
        new(0xd26f, 0xbc60),
        new(0xd271, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels),
        new(0xd273, 0xbcd2),
        new(0xd275, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31),
        new(0xd277, 0xd193),
        new(0xd279, TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo),
        new(0xd27b, 0xd10d),
        new(0xd27d, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd27f, 0x000c),
        new(0xd281, 0x0004),
        new(0xd285, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd287, 0x000e),
        new(0xd289, 0x0004),
        new(0xd28d, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789),
        new(0xd28f, 0xd031),
        new(0xd291, TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels),
        new(0xd293, 0xbcd2),
        new(0xd295, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd297, 0x0010),
        new(0xd299, 0x0004),
        new(0xd29d, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd29f, 0x0012),
        new(0xd2a1, 0x0004),
        new(0xd2a5, TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY),
        new(0xd2a7, 0x0000),
        new(0xd2a9, CommonEnemyInstructionCodes.Goto),
        new(0xd2ab, GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xd21b, 0xd233, 0xd23b, 0xd24b, 0xd253,
        0xd267, 0xd283, 0xd28b, 0xd29b, 0xd2a3,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoWalkingMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoWalkingMechanicsWord word in Words)
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
        foreach (GoldenTorizoWalkingMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}

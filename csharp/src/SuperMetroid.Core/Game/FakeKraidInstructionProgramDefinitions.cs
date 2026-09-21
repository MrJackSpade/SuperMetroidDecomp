namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct FakeKraidInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Fake Kraid's walking, action-selection, and spit programs.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class FakeKraidInstructionProgramDefinitions
{
    /// <summary><c>InstList_MiniKraid_ChooseAction</c> at $A6:99AC.</summary>
    internal const ushort ChooseActionFacingLeft = 0x99ac;
    /// <summary><c>InstList_MiniKraid_StepForwards_FacingLeft</c> at $A6:99AE.</summary>
    internal const ushort StepForwardsFacingLeft = 0x99ae;
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate</c> at $A6:99C4.</summary>
    internal const ushort ChooseAlternateActionFacingLeft = 0x99c4;
    /// <summary><c>InstList_MiniKraid_StepBackwards_FacingLeft</c> at $A6:99C6.</summary>
    internal const ushort StepBackwardsFacingLeft = 0x99c6;
    /// <summary><c>InstList_MiniKraid_FireSpit_FacingLeft</c> at $A6:99DC.</summary>
    internal const ushort FireSpitFacingLeft = 0x99dc;
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate_again2</c> at $A6:99FA.</summary>
    internal const ushort ChooseActionFacingRight = 0x99fa;
    /// <summary><c>InstList_MiniKraid_StepForwards_FacingRight</c> at $A6:99FC.</summary>
    internal const ushort StepForwardsFacingRight = 0x99fc;
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate_again3</c> at $A6:9A12.</summary>
    internal const ushort ChooseAlternateActionFacingRight = 0x9a12;
    /// <summary><c>InstList_MiniKraid_StepBackwards_FacingRight</c> at $A6:9A14.</summary>
    internal const ushort StepBackwardsFacingRight = 0x9a14;
    /// <summary><c>InstList_MiniKraid_FireSpit_FacingRight</c> at $A6:9A2A.</summary>
    internal const ushort FireSpitFacingRight = 0x9a2a;

    private static readonly FakeKraidInstructionMechanicsWord[] Words =
    [
        new(0x99ac, EnemyInstructionCodePointers.Instruction_MiniKraid_ChooseAction),
        new(0x99ae, 0x0010), new(0x99b2, 0x000c),
        new(0x99b6, 0x0008), new(0x99ba, 0x000c),
        new(0x99be, EnemyInstructionCodePointers.Instruction_MiniKraid_Move),
        new(0x99c0, CommonEnemyInstructionCodes.Goto),
        new(0x99c2, ChooseActionFacingLeft),

        new(0x99c4, EnemyInstructionCodePointers.Instruction_MiniKraid_ChooseAction),
        new(0x99c6, 0x0010),
        new(0x99ca, EnemyInstructionCodePointers.Instruction_MiniKraid_Move),
        new(0x99cc, 0x000c), new(0x99d0, 0x0008), new(0x99d4, 0x000c),
        new(0x99d8, CommonEnemyInstructionCodes.Goto),
        new(0x99da, ChooseAlternateActionFacingLeft),

        new(0x99dc, 0x0010),
        new(0x99e0, EnemyInstructionCodePointers.Instruction_MiniKraid_PlayCrySFX),
        new(0x99e2, 0x0008),
        new(0x99e6, EnemyInstructionCodePointers.Instruction_MiniKraid_FireSpitLeft),
        new(0x99e8, 0x0010), new(0x99ec, 0x0008),
        new(0x99f0, CommonEnemyInstructionCodes.Goto),
        new(0x99f2, ChooseActionFacingLeft),

        new(0x99fa, EnemyInstructionCodePointers.Instruction_MiniKraid_ChooseAction),
        new(0x99fc, 0x0010), new(0x9a00, 0x000c),
        new(0x9a04, 0x0008), new(0x9a08, 0x000c),
        new(0x9a0c, EnemyInstructionCodePointers.Instruction_MiniKraid_Move),
        new(0x9a0e, CommonEnemyInstructionCodes.Goto),
        new(0x9a10, ChooseActionFacingRight),

        new(0x9a12, EnemyInstructionCodePointers.Instruction_MiniKraid_ChooseAction),
        new(0x9a14, 0x0010),
        new(0x9a18, EnemyInstructionCodePointers.Instruction_MiniKraid_Move),
        new(0x9a1a, 0x000c), new(0x9a1e, 0x0008), new(0x9a22, 0x000c),
        new(0x9a26, CommonEnemyInstructionCodes.Goto),
        new(0x9a28, ChooseAlternateActionFacingRight),

        new(0x9a2a, 0x0010),
        new(0x9a2e, EnemyInstructionCodePointers.Instruction_MiniKraid_PlayCrySFX),
        new(0x9a30, 0x0008),
        new(0x9a34, EnemyInstructionCodePointers.Instruction_MiniKraid_FireSpitRight),
        new(0x9a36, 0x0010), new(0x9a3a, 0x0008),
        new(0x9a3e, CommonEnemyInstructionCodes.Goto),
        new(0x9a40, ChooseActionFacingRight),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x99b0, 0x99b4, 0x99b8, 0x99bc,
        0x99c8, 0x99ce, 0x99d2, 0x99d6,
        0x99de, 0x99e4, 0x99ea, 0x99ee,
        0x99fe, 0x9a02, 0x9a06, 0x9a0a,
        0x9a16, 0x9a1c, 0x9a20, 0x9a24,
        0x9a2c, 0x9a32, 0x9a38, 0x9a3c,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static FakeKraidInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Fake Kraid control or rejects pointers outside the live programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            FakeKraidInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Fake Kraid instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

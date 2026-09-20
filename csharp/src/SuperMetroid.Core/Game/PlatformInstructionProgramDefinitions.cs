namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct PlatformInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Tripper and Kamer's moving and vertically-still loops.
/// Their thirty-two interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class PlatformInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kamer2_VerticallyMoving_Left_0</c> at $A3:9BBB.</summary>
    internal const ushort KamerMovingLeft = 0x9bbb;
    /// <summary><c>InstList_Kamer2_VerticallyMoving_Right_0</c> at $A3:9BD1.</summary>
    internal const ushort KamerMovingRight = 0x9bd1;
    /// <summary><c>InstList_Kamer2_VerticallyStill_Left_0</c> at $A3:9BE7.</summary>
    internal const ushort KamerStillLeft = 0x9be7;
    /// <summary><c>InstList_Kamer2_VerticallyStill_Right_0</c> at $A3:9BFD.</summary>
    internal const ushort KamerStillRight = 0x9bfd;
    /// <summary><c>InstList_Tripper_VerticallyMoving_Left_0</c> at $A3:9C13.</summary>
    internal const ushort TripperMovingLeft = 0x9c13;
    /// <summary><c>InstList_Tripper_VerticallyMoving_Right_0</c> at $A3:9C29.</summary>
    internal const ushort TripperMovingRight = 0x9c29;
    /// <summary>
    /// <c>InstList_Tripper_VerticallyStill_Right_0</c> at $A3:9C3F. The native art label
    /// is opposite its published leftward horizontal movement, so this mechanics-facing
    /// name follows the callback effect used by the translated AI.
    /// </summary>
    internal const ushort TripperStillMovingLeft = 0x9c3f;
    /// <summary>
    /// <c>InstList_Tripper_VerticallyStill_Left_0</c> at $A3:9C55. The native art label
    /// is opposite its published rightward horizontal movement.
    /// </summary>
    internal const ushort TripperStillMovingRight = 0x9c55;
    /// <summary>The first callback implementation immediately after the programs.</summary>
    internal const ushort FirstAdjacentCallback = 0x9c6b;

    private static readonly PlatformInstructionMechanicsWord[] Words =
    [
        new(KamerMovingLeft,
            EnemyInstructionCodePointers.Instruction_Tripper_Kamer2_SetMovingLeftXMovement_duplicate),
        new(0x9bbd, 10), new(0x9bc1, 10), new(0x9bc5, 10), new(0x9bc9, 10),
        new(0x9bcd, CommonEnemyInstructionCodes.Goto), new(0x9bcf, 0x9bbd),

        new(KamerMovingRight,
            EnemyInstructionCodePointers.Instruction_Tripper_Kamer2_SetMovingRightXMovement_duplicate),
        new(0x9bd3, 10), new(0x9bd7, 10), new(0x9bdb, 10), new(0x9bdf, 10),
        new(0x9be3, CommonEnemyInstructionCodes.Goto), new(0x9be5, 0x9bd3),

        new(KamerStillLeft,
            EnemyInstructionCodePointers.Instruction_Tripper_Kamer2_SetMovingLeftXMovement),
        new(0x9be9, 10), new(0x9bed, 10), new(0x9bf1, 10), new(0x9bf5, 10),
        new(0x9bf9, CommonEnemyInstructionCodes.Goto), new(0x9bfb, 0x9be9),

        new(KamerStillRight,
            EnemyInstructionCodePointers.Instruction_Tripper_Kamer2_SetMovingRightXMovement),
        new(0x9bff, 10), new(0x9c03, 10), new(0x9c07, 10), new(0x9c0b, 10),
        new(0x9c0f, CommonEnemyInstructionCodes.Goto), new(0x9c11, 0x9bff),

        new(TripperMovingLeft,
            EnemyInstructionCodePointers.Instruction_Tripper_Kamer2_SetMovingLeftXMovement_duplicate),
        new(0x9c15, 7), new(0x9c19, 8), new(0x9c1d, 7), new(0x9c21, 8),
        new(0x9c25, CommonEnemyInstructionCodes.Goto), new(0x9c27, 0x9c15),

        new(TripperMovingRight,
            EnemyInstructionCodePointers.Instruction_Tripper_Kamer2_SetMovingRightXMovement_duplicate),
        new(0x9c2b, 7), new(0x9c2f, 8), new(0x9c33, 7), new(0x9c37, 8),
        new(0x9c3b, CommonEnemyInstructionCodes.Goto), new(0x9c3d, 0x9c2b),

        new(TripperStillMovingLeft,
            EnemyInstructionCodePointers.Instruction_Tripper_Kamer2_SetMovingLeftXMovement),
        new(0x9c41, 7), new(0x9c45, 8), new(0x9c49, 7), new(0x9c4d, 8),
        new(0x9c51, CommonEnemyInstructionCodes.Goto), new(0x9c53, 0x9c41),

        new(TripperStillMovingRight,
            EnemyInstructionCodePointers.Instruction_Tripper_Kamer2_SetMovingRightXMovement),
        new(0x9c57, 7), new(0x9c5b, 8), new(0x9c5f, 7), new(0x9c63, 8),
        new(0x9c67, CommonEnemyInstructionCodes.Goto), new(0x9c69, 0x9c57),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x9bbf, 0x9bc3, 0x9bc7, 0x9bcb,
        0x9bd5, 0x9bd9, 0x9bdd, 0x9be1,
        0x9beb, 0x9bef, 0x9bf3, 0x9bf7,
        0x9c01, 0x9c05, 0x9c09, 0x9c0d,
        0x9c17, 0x9c1b, 0x9c1f, 0x9c23,
        0x9c2d, 0x9c31, 0x9c35, 0x9c39,
        0x9c43, 0x9c47, 0x9c4b, 0x9c4f,
        0x9c59, 0x9c5d, 0x9c61, 0x9c65,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static PlatformInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PlatformInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Tripper/Kamer instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
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

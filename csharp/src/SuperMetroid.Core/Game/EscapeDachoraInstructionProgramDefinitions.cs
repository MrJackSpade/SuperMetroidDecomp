namespace SuperMetroid.Core.Game;

/// <summary>One compiled escape-Dachora mechanics word at its bank-$B3 address.</summary>
internal readonly record struct EscapeDachoraInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control words for the escape-sequence Dachora's low/high-tide pacing and
/// accelerating departure programs. Spritemap operands remain cartridge data.
/// </summary>
internal static class EscapeDachoraInstructionProgramDefinitions
{
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_0</c> at $B3:E964.</summary>
    internal const ushort RunningAroundLowTide = 0xe964;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_1</c> at $B3:E968.</summary>
    internal const ushort RunningAroundLowTideLeft = 0xe968;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_2</c> at $B3:E99C.</summary>
    internal const ushort RunningAroundLowTideRight = 0xe99c;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_0</c> at $B3:E9D0.</summary>
    internal const ushort RunningAroundHighTide = 0xe9d0;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_1</c> at $B3:E9D4.</summary>
    internal const ushort RunningAroundHighTideLeft = 0xe9d4;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_2</c> at $B3:E9FC.</summary>
    internal const ushort RunningAroundHighTideLeftLoop = 0xe9fc;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_3</c> at $B3:EA04.</summary>
    internal const ushort RunningAroundHighTideRight = 0xea04;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_4</c> at $B3:EA2C.</summary>
    internal const ushort RunningAroundHighTideRightLoop = 0xea2c;
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_0</c> at $B3:EA34.</summary>
    internal const ushort RunningForEscape = 0xea34;
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_1</c> at $B3:EA38.</summary>
    internal const ushort RunningForEscapeAccelerating = 0xea38;
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_2</c> at $B3:EA80.</summary>
    internal const ushort RunningForEscapeMaximumSpeed = 0xea80;
    /// <summary><c>InstList_DachoraEscape_GotoY_IfAcidLessThanCE</c>, adjacent code at $B3:EAA8.</summary>
    internal const ushort FirstAdjacentCodeRoutine = 0xeaa8;

    private static readonly EscapeDachoraInstructionMechanicsWord[] Words =
    [
        new(RunningAroundLowTide, CommonEnemyInstructionCodes.SetTimer),
        new(0xe966, 0x0005),
        new(RunningAroundLowTideLeft, 0x0003),
        new(0xe96c, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe96e, 0x0003),
        new(0xe972, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe974, 0x0003),
        new(0xe978, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe97a, 0x0003),
        new(0xe97e, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe980, 0x0003),
        new(0xe984, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe986, 0x0003),
        new(0xe98a, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe98c, EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfAcidLessThanCE),
        new(0xe98e, RunningAroundHighTideLeftLoop),
        new(0xe990, EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfCrittersEscaped),
        new(0xe992, RunningForEscape),
        new(0xe994, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xe996, RunningAroundLowTideLeft),
        new(0xe998, CommonEnemyInstructionCodes.SetTimer), new(0xe99a, 0x0005),
        new(RunningAroundLowTideRight, 0x0003),
        new(0xe9a0, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xe9a2, 0x0003),
        new(0xe9a6, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xe9a8, 0x0003),
        new(0xe9ac, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xe9ae, 0x0003),
        new(0xe9b2, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xe9b4, 0x0003),
        new(0xe9b8, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xe9ba, 0x0003),
        new(0xe9be, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xe9c0, EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfAcidLessThanCE),
        new(0xe9c2, RunningAroundHighTideRightLoop),
        new(0xe9c4, EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfCrittersEscaped),
        new(0xe9c6, RunningForEscapeAccelerating),
        new(0xe9c8, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xe9ca, RunningAroundLowTideRight),
        new(0xe9cc, CommonEnemyInstructionCodes.Goto), new(0xe9ce, RunningAroundLowTide),

        new(RunningAroundHighTide, CommonEnemyInstructionCodes.SetTimer),
        new(0xe9d2, 0x0005),
        new(RunningAroundHighTideLeft, 0x0002),
        new(0xe9d8, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe9da, 0x0002),
        new(0xe9de, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe9e0, 0x0002),
        new(0xe9e4, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe9e6, 0x0002),
        new(0xe9ea, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe9ec, 0x0002),
        new(0xe9f0, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe9f2, 0x0002),
        new(0xe9f6, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6),
        new(0xe9f8, EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfCrittersEscaped),
        new(0xe9fa, RunningForEscape),
        new(RunningAroundHighTideLeftLoop,
            CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xe9fe, RunningAroundHighTideLeft),
        new(0xea00, CommonEnemyInstructionCodes.SetTimer), new(0xea02, 0x0005),
        new(RunningAroundHighTideRight, 0x0002),
        new(0xea08, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea0a, 0x0002),
        new(0xea0e, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea10, 0x0002),
        new(0xea14, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea16, 0x0002),
        new(0xea1a, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea1c, 0x0002),
        new(0xea20, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea22, 0x0002),
        new(0xea26, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea28, EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfCrittersEscaped),
        new(0xea2a, RunningForEscapeAccelerating),
        new(RunningAroundHighTideRightLoop,
            CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xea2e, RunningAroundHighTideRight),
        new(0xea30, CommonEnemyInstructionCodes.Goto), new(0xea32, RunningAroundHighTide),

        new(RunningForEscape, 0x001e),
        new(RunningForEscapeAccelerating, 0x005a),
        new(0xea3c, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea3e, 0x0005),
        new(0xea42, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea44, 0x0005),
        new(0xea48, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea4a, 0x0004),
        new(0xea4e, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea50, 0x0004),
        new(0xea54, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea56, 0x0004),
        new(0xea5a, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea5c, 0x0003),
        new(0xea60, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea62, 0x0003),
        new(0xea66, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea68, 0x0003),
        new(0xea6c, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea6e, 0x0002),
        new(0xea72, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea74, 0x0002),
        new(0xea78, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea7a, 0x0002),
        new(0xea7e, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(RunningForEscapeMaximumSpeed, 0x0001),
        new(0xea84, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea86, 0x0001),
        new(0xea8a, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea8c, 0x0001),
        new(0xea90, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea92, 0x0001),
        new(0xea96, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea98, 0x0001),
        new(0xea9c, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xea9e, 0x0001),
        new(0xeaa2, EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6),
        new(0xeaa4, CommonEnemyInstructionCodes.Goto),
        new(0xeaa6, RunningForEscapeMaximumSpeed),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe96a, 0xe970, 0xe976, 0xe97c, 0xe982, 0xe988,
        0xe99e, 0xe9a4, 0xe9aa, 0xe9b0, 0xe9b6, 0xe9bc,
        0xe9d6, 0xe9dc, 0xe9e2, 0xe9e8, 0xe9ee, 0xe9f4,
        0xea06, 0xea0c, 0xea12, 0xea18, 0xea1e, 0xea24,
        0xea36, 0xea3a, 0xea40, 0xea46, 0xea4c, 0xea52, 0xea58,
        0xea5e, 0xea64, 0xea6a, 0xea70, 0xea76, 0xea7c,
        0xea82, 0xea88, 0xea8e, 0xea94, 0xea9a, 0xeaa0,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static EscapeDachoraInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EscapeDachoraInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }
        throw new InvalidDataException(
            $"Escape Dachora instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
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

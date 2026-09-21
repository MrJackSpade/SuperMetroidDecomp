namespace SuperMetroid.Core.Game;

/// <summary>One compiled escape-Etecoon mechanics word at its bank-$B3 address.</summary>
internal readonly record struct EscapeEtecoonInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control words for the escape-sequence Etecoon's low/high-tide walking,
/// waiting, gratitude, and departure programs. Spritemap operands remain cartridge data.
/// </summary>
internal static class EscapeEtecoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_EtecoonEscape_RunningLeft_LowTide_0</c> at $B3:E556.</summary>
    internal const ushort RunningLeftLowTide = 0xe556;
    /// <summary><c>InstList_EtecoonEscape_RunningLeft_HighTide</c> at $B3:E56E.</summary>
    internal const ushort RunningLeftHighTide = 0xe56e;
    /// <summary><c>InstList_EtecoonEscape_RunningRight_LowTide_0</c> at $B3:E582.</summary>
    internal const ushort RunningRightLowTide = 0xe582;
    /// <summary><c>InstList_EtecoonEscape_RunningRight_HighTide</c> at $B3:E59A.</summary>
    internal const ushort RunningRightHighTide = 0xe59a;
    /// <summary><c>InstList_EtecoonEscape_RunningForEscape_0</c> at $B3:E5AE.</summary>
    internal const ushort RunningForEscape = 0xe5ae;
    /// <summary><c>InstList_EtecoonEscape_Stationary</c> at $B3:E5C6.</summary>
    internal const ushort Stationary = 0xe5c6;
    /// <summary><c>InstList_EtecoonEscape_ExpressGratitudeThenEscape_0</c> at $B3:E5DA.</summary>
    internal const ushort ExpressGratitudeThenEscape = 0xe5da;
    /// <summary><c>Instruction_EtecoonEscape_XPositionPlusY</c>, adjacent code at $B3:E610.</summary>
    internal const ushort FirstAdjacentCodeRoutine = 0xe610;

    private static readonly EscapeEtecoonInstructionMechanicsWord[] Words =
    [
        new(RunningLeftLowTide,
            EscapeAnimalInstructionCodes.Instruction_EtecoonEscape_GotoY_IfAcidPositionLessThanCE),
        new(0xe558, RunningLeftHighTide),
        new(0xe55a, 0x0005), new(0xe55e, 0x0005),
        new(0xe562, 0x0005), new(0xe566, 0x0005),
        new(0xe56a, CommonEnemyInstructionCodes.Goto), new(0xe56c, 0xe55a),
        new(RunningLeftHighTide, 0x0003), new(0xe572, 0x0003),
        new(0xe576, 0x0003), new(0xe57a, 0x0003),
        new(0xe57e, CommonEnemyInstructionCodes.Goto),
        new(0xe580, RunningLeftHighTide),

        new(RunningRightLowTide,
            EscapeAnimalInstructionCodes.Instruction_EtecoonEscape_GotoY_IfAcidPositionLessThanCE),
        new(0xe584, RunningRightHighTide),
        new(0xe586, 0x0006), new(0xe58a, 0x0006),
        new(0xe58e, 0x0006), new(0xe592, 0x0006),
        new(0xe596, CommonEnemyInstructionCodes.Goto), new(0xe598, 0xe586),
        new(RunningRightHighTide, 0x0003), new(0xe59e, 0x0003),
        new(0xe5a2, 0x0003), new(0xe5a6, 0x0003),
        new(0xe5aa, CommonEnemyInstructionCodes.Goto),
        new(0xe5ac, RunningRightHighTide),

        new(RunningForEscape,
            EscapeAnimalInstructionCodes.Instruction_CommonB3_Enemy0FB2_InY),
        new(0xe5b0, (ushort)EscapeEtecoonPreInstruction.EscapeRight),
        new(0xe5b2, 0x0003), new(0xe5b6, 0x0003),
        new(0xe5ba, 0x0003), new(0xe5be, 0x0003),
        new(0xe5c2, CommonEnemyInstructionCodes.Goto), new(0xe5c4, 0xe5b2),

        new(Stationary, 0x0040), new(0xe5ca, 0x0008),
        new(0xe5ce, 0x0040), new(0xe5d2, 0x0008),
        new(0xe5d6, CommonEnemyInstructionCodes.Goto), new(0xe5d8, Stationary),

        new(ExpressGratitudeThenEscape,
            EscapeAnimalInstructionCodes.Instruction_CommonB3_SetEnemy0FB2ToRTS),
        new(0xe5dc, CommonEnemyInstructionCodes.SetTimer), new(0xe5de, 0x0008),
        new(0xe5e0, 0x0008),
        new(0xe5e4, EscapeAnimalInstructionCodes.Instruction_EtecoonEscape_XPositionPlusY),
        new(0xe5e6, 0xfffd), new(0xe5e8, 0x0008),
        new(0xe5ec, EscapeAnimalInstructionCodes.Instruction_EtecoonEscape_XPositionPlusY),
        new(0xe5ee, 0xfffd), new(0xe5f0, 0x0008),
        new(0xe5f4, EscapeAnimalInstructionCodes.Instruction_EtecoonEscape_XPositionPlusY),
        new(0xe5f6, 0xfffd), new(0xe5f8, 0x0008),
        new(0xe5fc, EscapeAnimalInstructionCodes.Instruction_EtecoonEscape_XPositionPlusY),
        new(0xe5fe, 0xfffd),
        new(0xe600, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xe602, 0xe5e0), new(0xe604, 0x0040), new(0xe608, 0x0008),
        new(0xe60c, CommonEnemyInstructionCodes.Goto),
        new(0xe60e, RunningForEscape),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe55c, 0xe560, 0xe564, 0xe568,
        0xe570, 0xe574, 0xe578, 0xe57c,
        0xe588, 0xe58c, 0xe590, 0xe594,
        0xe59c, 0xe5a0, 0xe5a4, 0xe5a8,
        0xe5b4, 0xe5b8, 0xe5bc, 0xe5c0,
        0xe5c8, 0xe5cc, 0xe5d0, 0xe5d4,
        0xe5e2, 0xe5ea, 0xe5f2, 0xe5fa, 0xe606, 0xe60a,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static EscapeEtecoonInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EscapeEtecoonInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Escape Etecoon instruction mechanics pointer $B3:{address:X4} is not compiled.");
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
